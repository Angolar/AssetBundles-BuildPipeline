﻿using UnityEditor.Build.Cache;
using UnityEditor.Build.Utilities;
using UnityEditor.Experimental.Build.AssetBundle;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.Build.AssetBundle.DataConverters
{
    public class AddressableAssetPacker : IDataConverter<AddressableAssetEntry[], BuildInput>
    {
        public uint Version { get { return 1; } }

        private Hash128 CalculateInputHash(AddressableAssetEntry[] input)
        {
            return HashingMethods.CalculateMD5Hash(Version, input);
        }

        public bool Convert(AddressableAssetEntry[] input, out BuildInput output, bool useCache = true)
        {
            // If enabled, try loading from cache
            Hash128 hash = new Hash128();
            if (useCache)
            {
                hash = CalculateInputHash(input);
                if(LoadFromCache(hash, out output))
                    return true;
            }
            
            // Convert inputs
            output = new BuildInput();

            if (input.IsNullOrEmpty())
            {
                BuildLogger.LogError("Unable to continue packing. Input is null or empty!");
                return false;
            }

            output.definitions = new BuildInput.Definition[input.Length];
            for (var index = 0; index < input.Length; index++)
            {
                var entry = input[index];
                var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid.ToString());
                var address = string.IsNullOrEmpty(entry.address) ? assetPath : entry.address;
                output.definitions[index].assetBundleName = address;
                output.definitions[index].explicitAssets = new[] { new BuildInput.AddressableAsset() { asset = entry.guid, address = address } };
            }
            
            // Cache results
            if (useCache)
                SaveToCache(hash, output);
            return true;
        }

        private bool LoadFromCache(Hash128 hash, out BuildInput output)
        {
            return BuildCache.TryLoadCachedResults(hash, out output);
        }

        private void SaveToCache(Hash128 hash, BuildInput output)
        {
            BuildCache.SaveCachedResults(hash, output);
        }
    }
}
