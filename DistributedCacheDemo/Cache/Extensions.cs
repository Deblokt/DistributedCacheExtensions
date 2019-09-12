using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedCacheDemo.Cache
{
    public static class Extensions
    {
        public async static Task<T> FromAsync<T>(this IDistributedCache distributedCache, Tag.Name tag, object parameters, Func<Task<T>> query, DistributedCacheEntryOptions options, CancellationToken token = default) where T : class
        {
            T result = await distributedCache.GetAsync<T>(tag, parameters, token);
            if (result == null)
            {
                result = await query.Invoke();
                await SetAsync<T>(distributedCache, tag, parameters, result, options, token);
            }
            return result;
        }

        public async static Task SetAsync<T>(this IDistributedCache distributedCache, Tag.Name tag, object parameters, T value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            string cacheKey = GetCacheKey(tag, parameters);
            await distributedCache.SetAsync(cacheKey, value.ToByteArray(), options, token);
            await InsertHashIntoTagMap(distributedCache, tag, cacheKey, token);
        }

        public async static Task<T> GetAsync<T>(this IDistributedCache distributedCache, Tag.Name tag, object parameters, CancellationToken token = default) where T : class
        {
            string cacheKey = GetCacheKey(tag, parameters);
            var result = await distributedCache.GetAsync(cacheKey, token);
            return result.FromByteArray<T>();
        }

        public async static Task RemoveAsync(this IDistributedCache distributedCache, Tag.Name tag, CancellationToken token = default)
        {
            await RemoveHashesAndTagMap(distributedCache, tag, token);
        }

        public async static Task RemoveAsync(this IDistributedCache distributedCache, params Tag.Name[] tags)
        {
            if(tags != null && tags.Any())
            {
                foreach(var tag in tags)
                {
                    await RemoveHashesAndTagMap(distributedCache, tag);
                }
            }            
        }

        public async static Task RemoveAsync(this IDistributedCache distributedCache, Tag.Name tag, object parameters, CancellationToken token = default)
        {
            string cacheKey = GetCacheKey(tag, parameters);
            await distributedCache.RemoveAsync(cacheKey, token);
            await RemoveHashFromTagMap(distributedCache, tag, cacheKey, token);
        }

        #region Helpers
        private static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }
        private static T FromByteArray<T>(this byte[] byteArray) where T : class
        {
            if (byteArray == null)
            {
                return default(T);
            }
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                return binaryFormatter.Deserialize(memoryStream) as T;
            }
        }

        private static string GetCacheKey(Tag.Name tag, object parameters = null)
        {
            string _tag = tag.ToString();
            string _parameters = string.Empty;
            if (parameters != null)
            {
                _parameters = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
            }
            return SHA256(string.Format("{0}{1}", _tag, _parameters));
        }

        private static string SHA256(string text)
        {
            using (var algo = new SHA256Managed())
            {
                algo.ComputeHash(Encoding.UTF8.GetBytes(text));
                var result = algo.Hash;
                return string.Join(
                    string.Empty,
                    result.Select(x => x.ToString("x2")));
            }
        }

        public async static Task InsertHashIntoTagMap(IDistributedCache distributedCache, Tag.Name tag, string cacheKey, CancellationToken token = default)
        {
            string masterKey = string.Format("{0}{1}", "master", tag.ToString());
            var hashes = (await distributedCache.GetAsync(masterKey, token)).FromByteArray<List<string>>();
            if (hashes == null)
            {
                hashes = new List<string>();
            }
            if (!hashes.Contains(cacheKey))
            {
                hashes.Add(cacheKey);
                await distributedCache.SetAsync(masterKey, hashes.ToByteArray(), new DistributedCacheEntryOptions() { AbsoluteExpiration = null }, token);
            }
        }

        public async static Task RemoveHashFromTagMap(IDistributedCache distributedCache, Tag.Name tag, string cacheKey, CancellationToken token = default)
        {
            string masterKey = string.Format("{0}{1}", "master", tag.ToString());
            var hashes = (await distributedCache.GetAsync(masterKey, token)).FromByteArray<List<string>>();
            if (hashes == null)
            {
                hashes = new List<string>();
            }
            if (hashes.Contains(cacheKey))
            {
                hashes.Remove(cacheKey);
                await distributedCache.SetAsync(masterKey, hashes.ToByteArray(), new DistributedCacheEntryOptions() { AbsoluteExpiration = null }, token);
            }
        }

        public async static Task RemoveHashesAndTagMap(IDistributedCache distributedCache, Tag.Name tag, CancellationToken token = default)
        {
            string masterKey = string.Format("{0}{1}", "master", tag.ToString());
            var hashes = (await distributedCache.GetAsync(masterKey, token)).FromByteArray<List<string>>();
            if (hashes == null)
            {
                hashes = new List<string>();
            }
            foreach (var hash in hashes)
            {
                await distributedCache.RemoveAsync(hash, token);
            }
            await distributedCache.RemoveAsync(masterKey, token);
        }
        #endregion
    }
}
