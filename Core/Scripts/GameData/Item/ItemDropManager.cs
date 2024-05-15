﻿using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class ItemDropManager
    {
        [ArrayElementTitle("item")]
        public ItemDrop[] randomItems = new ItemDrop[0];
        public ItemDropTable[] itemDropTables = new ItemDropTable[0];
        public ItemRandomByWeightTable[] itemRandomByWeightTables = new ItemRandomByWeightTable[0];
        [Tooltip("Max kind of items that will be dropped in ground")]
        public byte maxDropItems = 5;

        [System.NonSerialized]
        private List<ItemRandomByWeightTable> _cacheItemRandomByWeightTables = null;
        public List<ItemRandomByWeightTable> CacheItemRandomByWeightTables
        {
            get
            {
                if (_cacheItemRandomByWeightTables == null)
                {
                    _cacheItemRandomByWeightTables = new List<ItemRandomByWeightTable>();
                    if (itemRandomByWeightTables != null)
                        _cacheItemRandomByWeightTables.AddRange(itemRandomByWeightTables);
                }
                return _cacheItemRandomByWeightTables;
            }
        }

        [System.NonSerialized]
        private List<ItemDrop> _certainDropItems = new List<ItemDrop>();
        [System.NonSerialized]
        private List<ItemDrop> _uncertainDropItems = new List<ItemDrop>();

        [System.NonSerialized]
        private List<ItemDrop> _cacheRandomItems = null;
        public List<ItemDrop> CacheRandomItems
        {
            get
            {
                if (_cacheRandomItems == null)
                {
                    int i;
                    _cacheRandomItems = new List<ItemDrop>();
                    if (randomItems != null &&
                        randomItems.Length > 0)
                    {
                        for (i = 0; i < randomItems.Length; ++i)
                        {
                            if (randomItems[i].item == null ||
                                randomItems[i].maxAmount <= 0 ||
                                randomItems[i].dropRate <= 0)
                                continue;
                            _cacheRandomItems.Add(randomItems[i]);
                        }
                    }
                    if (itemDropTables != null &&
                        itemDropTables.Length > 0)
                    {
                        foreach (ItemDropTable itemDropTable in itemDropTables)
                        {
                            if (itemDropTable != null &&
                                itemDropTable.randomItems != null &&
                                itemDropTable.randomItems.Length > 0)
                            {
                                for (i = 0; i < itemDropTable.randomItems.Length; ++i)
                                {
                                    if (itemDropTable.randomItems[i].item == null ||
                                        itemDropTable.randomItems[i].maxAmount <= 0 ||
                                        itemDropTable.randomItems[i].dropRate <= 0)
                                        continue;
                                    _cacheRandomItems.Add(itemDropTable.randomItems[i]);
                                }
                            }
                        }
                    }
                    _cacheRandomItems.Sort((a, b) => b.dropRate.CompareTo(a.dropRate));
                    _certainDropItems.Clear();
                    _uncertainDropItems.Clear();
                    for (i = 0; i < _cacheRandomItems.Count; ++i)
                    {
                        if (_cacheRandomItems[i].dropRate >= 1f)
                            _certainDropItems.Add(_cacheRandomItems[i]);
                        else
                            _uncertainDropItems.Add(_cacheRandomItems[i]);
                    }
                }
                return _cacheRandomItems;
            }
        }

        public void PrepareRelatesData()
        {
            GameInstance.AddItems(CacheRandomItems);
            if (itemRandomByWeightTables != null)
            {
                foreach (ItemRandomByWeightTable entry in itemRandomByWeightTables)
                {
                    GameInstance.AddItems(entry.randomItems);
                }
            }
        }

        public virtual void RandomItems(System.Action<BaseItem, int> onRandomItem, float rate = 1f)
        {
            if (CacheRandomItems.Count == 0 && CacheItemRandomByWeightTables.Count <= 0)
                return;
            int randomDropCount = 0;
            int i;
            // Drop certain drop rate items
            _certainDropItems.Shuffle();
            for (i = 0; i < _certainDropItems.Count && randomDropCount < maxDropItems; ++i)
            {
                if (BaseGameNetworkManager.CurrentMapInfo.ExcludeItemFromDropping(_certainDropItems[i].item))
                    continue;
                if (_certainDropItems[i].minAmount <= 0)
                    onRandomItem.Invoke(_certainDropItems[i].item, _certainDropItems[i].maxAmount);
                else
                    onRandomItem.Invoke(_certainDropItems[i].item, Random.Range(_certainDropItems[i].minAmount, _certainDropItems[i].maxAmount));
                ++randomDropCount;
            }
            // Reached max drop items?
            if (randomDropCount >= maxDropItems)
                return;
            // Drop uncertain drop rate items
            _uncertainDropItems.Shuffle();
            for (i = 0; i < _uncertainDropItems.Count && randomDropCount < maxDropItems; ++i)
            {
                if (Random.value > _uncertainDropItems[i].dropRate * rate)
                    continue;
                if (BaseGameNetworkManager.CurrentMapInfo.ExcludeItemFromDropping(_uncertainDropItems[i].item))
                    continue;
                if (_uncertainDropItems[i].minAmount <= 0)
                    onRandomItem.Invoke(_uncertainDropItems[i].item, _uncertainDropItems[i].maxAmount);
                else
                    onRandomItem.Invoke(_uncertainDropItems[i].item, Random.Range(_uncertainDropItems[i].minAmount, _uncertainDropItems[i].maxAmount));
                ++randomDropCount;
            }
            // Reached max drop items?
            if (randomDropCount >= maxDropItems)
                return;
            // Drop items by weighted tables
            CacheItemRandomByWeightTables.Shuffle();
            for (i = 0; i < CacheItemRandomByWeightTables.Count && randomDropCount < maxDropItems; ++i)
            {
                CacheItemRandomByWeightTables[i].RandomItem(onRandomItem);
            }
        }
    }
}
