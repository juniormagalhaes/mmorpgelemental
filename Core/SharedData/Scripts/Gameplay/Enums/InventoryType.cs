﻿namespace MultiplayerARPG
{
    public enum InventoryType : byte
    {
        NonEquipItems,
        EquipItems,
        EquipWeaponRight,
        EquipWeaponLeft,
        StorageItems,
        ItemsContainer,
        Vending,
        Unknow = 254,
    }
}
