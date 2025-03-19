using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Object = UnityEngine.Object;


namespace PackagesEasyAR.Util
{
    public static class AddressableExtension
    {
#if UNITY_EDITOR
        /// <summary>
        /// 根据路径获取AssetEntry
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>Addressable中的资源项</returns>
        public static AddressableAssetEntry GetAssetEntryFromPath(string path)
        {
            var data = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(data))
                return null;
            return AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(data, true);
        }

        /// <summary>
        /// 根据资源项的address获取资源
        /// </summary>
        /// <param name="addr">address</param>
        /// <param name="group">所属group</param>
        /// <param name="includeSubObjects">包含子物体</param>
        /// <returns>资源项</returns>
        public static AddressableAssetEntry GetAssetEntryFromAddr(string addr, string group = null, bool includeSubObjects = false)
            => FindEntry(e => e.address == addr, group, includeSubObjects);

        public static bool TryGetAssetEntryFromAddr(string addr, out AddressableAssetEntry entry, string group = null, bool includeSubObjects = false)
        {
            var addressableAssetEntry = GetAssetEntryFromAddr(addr, group, includeSubObjects);
            entry = addressableAssetEntry;
            return addressableAssetEntry != null;
        }
        public static bool TryGetAssetEntryFromObjectByGuid(string guid, out AddressableAssetEntry entry)
        {
            entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid, true);
            if (string.IsNullOrEmpty(guid) || entry == null)
            {
                entry = null;
                return false;
            }

            return true;
        }

        public static bool TryGetAssetEntryFromObjectByPath(string path, out AddressableAssetEntry entry)
        {
            var data = AssetDatabase.AssetPathToGUID(path);
            entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(data, true);
            if (string.IsNullOrEmpty(data) || entry == null)
            {
                entry = null;
                return false;
            }

            return true;
        }
        /// <summary>
        /// 获取资源，使用bool+引用的方式
        /// </summary>
        /// <param name="res">资源</param>
        /// <param name="entry">对象</param>
        /// <returns>是否取得</returns>
        public static bool TryGetAssetEntryFromObject(Object res, out AddressableAssetEntry entry)
        {
            var path = AssetDatabase.GetAssetPath(res);
            var data = AssetDatabase.AssetPathToGUID(path);
            entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(data, true);
            if (string.IsNullOrEmpty(data) || entry == null)
            {
                entry = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 添加某个资源到组中
        /// </summary>
        /// <param name="res">资源</param>
        /// <param name="groupName">组名</param>
        /// <param name="addr">地址</param>
        /// <returns>Addressable中的资源项</returns>
        public static AddressableAssetEntry AddToAddressable(Object res, string groupName, string addr = "")
        {
            var path = AssetDatabase.GetAssetPath(res);
            var data = AssetDatabase.AssetPathToGUID(path);
            if (data == null)
            {
                Debug.LogError("Unresolved error on resource: " + res);
            }

            var setting = AddressableAssetSettingsDefaultObject.Settings;
            var group = setting.FindGroup(groupName);
            var entry = setting.CreateOrMoveEntry(data, group);
            if (!string.IsNullOrEmpty(addr))
            {
                entry.address = addr;
            }

            return entry;
        }

        public static void AddToAddressable(string guid, string groupName, string addr = "")
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            var group = setting.FindGroup(groupName);
            var entry = setting.CreateOrMoveEntry(guid, group);
            if (!string.IsNullOrEmpty(addr))
            {
                entry.address = addr;
            }
        }

        public static List<AddressableAssetGroup> GetAllGroups()
            => AddressableAssetSettingsDefaultObject.Settings.groups;

        /// <summary>
        /// 获取Addressable的组
        /// </summary>
        /// <param name="name">组名</param>
        /// <returns>组</returns>
        public static AddressableAssetGroup GetGroup(string name)
            => AddressableAssetSettingsDefaultObject.Settings.FindGroup(name);

        /// <summary>
        /// 在组中创建资源
        /// </summary>
        /// <param name="group">组</param>
        /// <param name="res">资源对象</param>
        /// <param name="addr">地址</param>
        /// <returns>Addressable中的资源项</returns>
        public static AddressableAssetEntry CreateAssetEntry(this AddressableAssetGroup group, Object res, string addr = "")
        {
            var path = AssetDatabase.GetAssetPath(res);
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError("Cannot resolve object! " + res);
                return null;
            }

            var setting = group.Settings;
            if (setting.FindAssetEntry(guid) != null)
            {
                Debug.LogWarning("The entry has already existed.." + res);
                return setting.FindAssetEntry(guid);
            }

            var entry = setting.CreateOrMoveEntry(guid, group);
            if (!string.IsNullOrEmpty(addr))
            {
                entry.address = addr;
            }

            return entry;
        }

        /// <summary>
        /// 移除资源项
        /// </summary>
        /// <param name="obj">资源</param>
        /// <returns>是否移除成功</returns>
        public static bool RemoveAssetEntry(Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            return RemoveAssetEntry(path);
        }

        /// <summary>
        /// 移除资源项
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>是否移除成功</returns>
        public static bool RemoveAssetEntry(string path)
        {
            var data = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(data))
            {
                Debug.LogError("Unresolved error on resource: " + path);
            }

            var setting = AddressableAssetSettingsDefaultObject.Settings;
            return setting.RemoveAssetEntry(data);
        }

        /// <summary>
        /// 查找资源项
        /// </summary>
        /// <param name="predicate">查找条件</param>
        /// <param name="group">组名</param>
        /// <param name="includeSubObjects">包含子物体，如sprite</param>
        /// <returns>符合条件的资源列表</returns>
        public static List<AddressableAssetEntry> FindEntries(Func<AddressableAssetEntry, bool> predicate, string group = null, bool includeSubObjects = true)
        {
            var def = AddressableAssetSettingsDefaultObject.Settings;
            var lst = new List<AddressableAssetEntry>();
            if (group == null)
                def.GetAllAssets(lst, includeSubObjects, entryFilter: predicate);
            else
                def.GetAllAssets(lst, includeSubObjects, groupFilter: e => e.Name.Equals(group), entryFilter: predicate);
            // Debug.Log("Include sub: " + includeSubObjects + " , " + lst.Count);
            return lst;
        }

        /// <summary>
        /// 查找资源项
        /// </summary>
        /// <param name="group">待查找组</param>
        /// <param name="predicate">查找条件</param>
        /// <param name="includeSubObjects">包含子物体，如sprite</param>
        /// <returns>符合条件的资源列表</returns>
        public static List<AddressableAssetEntry> FindEntries(this AddressableAssetGroup group, Func<AddressableAssetEntry, bool> predicate, bool includeSubObjects = true)
        {
            var def = AddressableAssetSettingsDefaultObject.Settings;
            var lst = new List<AddressableAssetEntry>();
            def.GetAllAssets(lst, includeSubObjects, groupFilter: group.Equals, entryFilter: predicate);
            return lst;
        }

        /// <summary>
        /// 查找资源项
        /// 如果没有符合条件的资源，会返回null；如果出现多个，会抛出异常。
        /// </summary>
        /// <param name="predicate">查找条件</param>
        /// <param name="group">待查找组</param>
        /// <param name="includeSubObjects">包含子物体，如sprite</param>
        /// <returns>符合条件的资源列表</returns>
        public static AddressableAssetEntry FindEntry(Func<AddressableAssetEntry, bool> predicate, string group = null, bool includeSubObjects = true)
            => FindEntries(predicate, group, includeSubObjects).SingleOrDefault();

        /// <summary>
        /// 查找资源项
        /// 如果没有符合条件的资源，会返回null；如果出现多个，会抛出异常。
        /// </summary>
        /// <param name="group">待查找组</param>
        /// <param name="predicate">查找条件</param>
        /// <param name="includeSubObjects">包含子物体，如sprite</param>
        /// <returns>符合条件的资源列表</returns>
        public static AddressableAssetEntry FindEntry(this AddressableAssetGroup group, Func<AddressableAssetEntry, bool> predicate, bool includeSubObjects = true)
            => group.FindEntries(predicate, includeSubObjects).SingleOrDefault();

        /// <summary>
        /// 将AssetDatabase地址转换为Addressable地址
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string AssetPathToAddressableAddr(string assetPath)
            => GetAssetEntryFromPath(assetPath).address;

        /// <summary>
        /// 批量添加或修改资源到某个组中
        /// </summary>
        /// <param name="objs">资源列表</param>
        /// <param name="nGroup">新的组</param>
        /// <returns>是否成功</returns>
        public static bool MoveOrAddAssetsToGroup(IEnumerable<Object> objs, AddressableAssetGroup nGroup)
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            foreach (var e in objs)
            {
                setting.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(e)), nGroup);
            }

            return true;
        }

        /// <summary>
        /// 批量添加或修改资源到某个组中
        /// </summary>
        /// <param name="objs">资源列表</param>
        /// <param name="nGroup">新的组</param>
        /// <returns>是否成功</returns>
        public static bool MoveOrAddAssetsToGroup(IEnumerable<string> objPaths, AddressableAssetGroup nGroup)
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            foreach (var e in objPaths)
            {
                setting.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(e), nGroup);
            }

            return true;
        }

        /// <summary>
        /// 批量添加或修改资源到某个组中
        /// </summary>
        /// <param name="objs">资源列表</param>
        /// <param name="nGroup">新的组</param>
        /// <returns>是否成功</returns>
        public static bool MoveOrAddAssetToGroup(Object obj, AddressableAssetGroup nGroup)
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            setting.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)), nGroup);
            return true;
        }

        /// <summary>
        /// 批量添加或修改资源到某个组中
        /// </summary>
        /// <param name="objs">资源列表</param>
        /// <param name="nGroup">新的组</param>
        /// <returns>是否成功</returns>
        public static bool MoveOrAddAssetToGroup(string objPath, AddressableAssetGroup nGroup)
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            setting.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(objPath), nGroup);
            return true;
        }

        /// <summary>
        /// 批量添加或修改资源到某个组中
        /// </summary>
        /// <param name="objs">资源列表</param>
        /// <param name="nGroup">新的组</param>
        /// <returns>是否成功</returns>
        public static bool MoveOrAddAssetToGroupWithAddr(string objAddr, AddressableAssetGroup nGroup)
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            setting.CreateOrMoveEntry(GetAssetEntryFromAddr(objAddr).guid, nGroup);
            return true;
        }

        /// <summary>
        /// 找到下一个可用名称。例如，王鼎、王鼎(1)已被使用，则返回王鼎(2)
        /// </summary>
        /// <param name="oriName">原来的地址</param>
        /// <returns>可用的地址</returns>
        public static string FindNextValidAddress(string oriName)
        {
            int id = 1;
            while (GetAssetEntryFromAddr(oriName) != null)
            {
                oriName = $"{oriName}({id})";
            }

            return oriName;
        }

#endif
    }
}