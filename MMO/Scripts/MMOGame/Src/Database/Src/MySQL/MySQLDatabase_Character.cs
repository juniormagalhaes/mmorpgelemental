﻿#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using Cysharp.Threading.Tasks;
using MySqlConnector;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        private async UniTask FillCharacterAttributes(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData)
        {
            try
            {
                await DeleteCharacterAttributes(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Attributes.Count; ++i)
                {
                    await CreateCharacterAttribute(connection, transaction, insertedIds, characterData.Id, characterData.Attributes[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing attributes of character: " + characterData.Id);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterBuffs(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData)
        {
            try
            {
                await DeleteCharacterBuffs(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Buffs.Count; ++i)
                {
                    await CreateCharacterBuff(connection, transaction, insertedIds, characterData.Id, characterData.Buffs[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing buffs of character: " + characterData.Id);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterHotkeys(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData)
        {
            try
            {
                await DeleteCharacterHotkeys(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Hotkeys.Count; ++i)
                {
                    await CreateCharacterHotkey(connection, transaction, insertedIds, characterData.Id, characterData.Hotkeys[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing hotkeys of character: " + characterData.Id);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterItems(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData)
        {
            try
            {
                await DeleteCharacterItems(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.SelectableWeaponSets.Count; ++i)
                {
                    await CreateCharacterEquipWeapons(connection, transaction, insertedIds, i, characterData.Id, characterData.SelectableWeaponSets[i]);
                }
                for (i = 0; i < characterData.EquipItems.Count; ++i)
                {
                    await CreateCharacterEquipItem(connection, transaction, insertedIds, i, characterData.Id, characterData.EquipItems[i]);
                }
                for (i = 0; i < characterData.NonEquipItems.Count; ++i)
                {
                    await CreateCharacterNonEquipItem(connection, transaction, insertedIds, i, characterData.Id, characterData.NonEquipItems[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing items of character: " + characterData.Id);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterQuests(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData)
        {
            try
            {
                await DeleteCharacterQuests(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Quests.Count; ++i)
                {
                    await CreateCharacterQuest(connection, transaction, insertedIds, characterData.Id, characterData.Quests[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing quests of character: " + characterData.Id);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterCurrencies(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData)
        {
            try
            {
                await DeleteCharacterCurrencies(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Currencies.Count; ++i)
                {
                    await CreateCharacterCurrency(connection, transaction, insertedIds, characterData.Id, characterData.Currencies[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing currencies of character: " + characterData.Id);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterSkills(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData)
        {
            try
            {
                await DeleteCharacterSkills(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Skills.Count; ++i)
                {
                    await CreateCharacterSkill(connection, transaction, insertedIds, characterData.Id, characterData.Skills[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing skills of character: " + characterData.Id);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterSkillUsages(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData)
        {
            try
            {
                await DeleteCharacterSkillUsages(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.SkillUsages.Count; ++i)
                {
                    await CreateCharacterSkillUsage(connection, transaction, insertedIds, characterData.Id, characterData.SkillUsages[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing skill usages of character: " + characterData.Id);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterSummons(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData)
        {
            try
            {
                await DeleteCharacterSummons(connection, transaction, characterData.Id);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < characterData.Summons.Count; ++i)
                {
                    await CreateCharacterSummon(connection, transaction, insertedIds, i, characterData.Id, characterData.Summons[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing skill usages of character: " + characterData.Id);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterDataBooleans(MySqlConnection connection, MySqlTransaction transaction, string tableName, string characterId, IList<CharacterDataBoolean> list)
        {
            try
            {
                await DeleteCharacterDataBooleans(connection, transaction, tableName, characterId);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < list.Count; ++i)
                {
                    await CreateCharacterDataBoolean(connection, transaction, tableName, insertedIds, characterId, list[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing custom boolean of character: " + characterId + ", table: " + tableName);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterDataInt32s(MySqlConnection connection, MySqlTransaction transaction, string tableName, string characterId, IList<CharacterDataInt32> list)
        {
            try
            {
                await DeleteCharacterDataInt32s(connection, transaction, tableName, characterId);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < list.Count; ++i)
                {
                    await CreateCharacterDataInt32(connection, transaction, tableName, insertedIds, characterId, list[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing custom int32 of character: " + characterId + ", table: " + tableName);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterDataFloat32s(MySqlConnection connection, MySqlTransaction transaction, string tableName, string characterId, IList<CharacterDataFloat32> list)
        {
            try
            {
                await DeleteCharacterDataFloat32s(connection, transaction, tableName, characterId);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < list.Count; ++i)
                {
                    await CreateCharacterDataFloat32(connection, transaction, tableName, insertedIds, characterId, list[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing custom float32 of character: " + characterId + ", table: " + tableName);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillSummonBuffs(MySqlConnection connection, MySqlTransaction transaction, string characterId, List<CharacterBuff> summonBuffs)
        {
            try
            {
                await DeleteSummonBuff(connection, transaction, characterId);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < summonBuffs.Count; ++i)
                {
                    await CreateSummonBuff(connection, transaction, insertedIds, characterId, summonBuffs[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing buffs of summon: " + characterId);
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillPlayerStorageItems(MySqlConnection connection, MySqlTransaction transaction, string userId, List<CharacterItem> storageItems)
        {
            try
            {
                StorageType storageType = StorageType.Player;
                string storageOwnerId = userId;
                await DeleteStorageItems(connection, transaction, storageType, storageOwnerId);
                HashSet<string> insertedIds = new HashSet<string>();
                int i;
                for (i = 0; i < storageItems.Count; ++i)
                {
                    await CreateStorageItem(connection, transaction, insertedIds, i, storageType, storageOwnerId, storageItems[i]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, "Transaction, Error occurs while replacing storage items");
                LogException(LogTag, ex);
                throw;
            }
        }

        private async UniTask FillCharacterRelatesData(MySqlConnection connection, MySqlTransaction transaction, IPlayerCharacterData characterData, List<CharacterBuff> summonBuffs, List<CharacterItem> storageItems)
        {
            await FillCharacterAttributes(connection, transaction, characterData);
            await FillCharacterBuffs(connection, transaction, characterData);
            await FillCharacterHotkeys(connection, transaction, characterData);
            await FillCharacterItems(connection, transaction, characterData);
            await FillCharacterQuests(connection, transaction, characterData);
            await FillCharacterCurrencies(connection, transaction, characterData);
            await FillCharacterSkills(connection, transaction, characterData);
            await FillCharacterSkillUsages(connection, transaction, characterData);
            await FillCharacterSummons(connection, transaction, characterData);

            await FillCharacterDataBooleans(connection, transaction, "character_server_boolean", characterData.Id, characterData.ServerBools);
            await FillCharacterDataInt32s(connection, transaction, "character_server_int32", characterData.Id, characterData.ServerInts);
            await FillCharacterDataFloat32s(connection, transaction, "character_server_float32", characterData.Id, characterData.ServerFloats);

            await FillCharacterDataBooleans(connection, transaction, "character_private_boolean", characterData.Id, characterData.PrivateBools);
            await FillCharacterDataInt32s(connection, transaction, "character_private_int32", characterData.Id, characterData.PrivateInts);
            await FillCharacterDataFloat32s(connection, transaction, "character_private_float32", characterData.Id, characterData.PrivateFloats);

            await FillCharacterDataBooleans(connection, transaction, "character_public_boolean", characterData.Id, characterData.PublicBools);
            await FillCharacterDataInt32s(connection, transaction, "character_public_int32", characterData.Id, characterData.PublicInts);
            await FillCharacterDataFloat32s(connection, transaction, "character_public_float32", characterData.Id, characterData.PublicFloats);

            if (summonBuffs != null)
                await FillSummonBuffs(connection, transaction, characterData.Id, summonBuffs);

            if (storageItems != null)
                await FillPlayerStorageItems(connection, transaction, characterData.UserId, storageItems);

        }

        public override async UniTask CreateCharacter(string userId, IPlayerCharacterData character)
        {
            using (MySqlConnection connection = NewConnection())
            {
                await OpenConnection(connection);
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await ExecuteNonQuery(connection, transaction, "INSERT INTO characters " +
                            "(id, userId, dataId, entityId, factionId, characterName, level, exp, currentHp, currentMp, currentStamina, currentFood, currentWater, equipWeaponSet, statPoint, skillPoint, gold, currentMapName, currentPositionX, currentPositionY, currentPositionZ, currentRotationX, currentRotationY, currentRotationZ, respawnMapName, respawnPositionX, respawnPositionY, respawnPositionZ, mountDataId, iconDataId, frameDataId, titleDataId) VALUES " +
                            "(@id, @userId, @dataId, @entityId, @factionId, @characterName, @level, @exp, @currentHp, @currentMp, @currentStamina, @currentFood, @currentWater, @equipWeaponSet, @statPoint, @skillPoint, @gold, @currentMapName, @currentPositionX, @currentPositionY, @currentPositionZ, @currentRotationX, @currentRotationY, @currentRotationZ, @respawnMapName, @respawnPositionX, @respawnPositionY, @respawnPositionZ, @mountDataId, @iconDataId, @frameDataId, @titleDataId)",
                            new MySqlParameter("@id", character.Id),
                            new MySqlParameter("@userId", userId),
                            new MySqlParameter("@dataId", character.DataId),
                            new MySqlParameter("@entityId", character.EntityId),
                            new MySqlParameter("@factionId", character.FactionId),
                            new MySqlParameter("@characterName", character.CharacterName),
                            new MySqlParameter("@level", character.Level),
                            new MySqlParameter("@exp", character.Exp),
                            new MySqlParameter("@currentHp", character.CurrentHp),
                            new MySqlParameter("@currentMp", character.CurrentMp),
                            new MySqlParameter("@currentStamina", character.CurrentStamina),
                            new MySqlParameter("@currentFood", character.CurrentFood),
                            new MySqlParameter("@currentWater", character.CurrentWater),
                            new MySqlParameter("@equipWeaponSet", character.EquipWeaponSet),
                            new MySqlParameter("@statPoint", character.StatPoint),
                            new MySqlParameter("@skillPoint", character.SkillPoint),
                            new MySqlParameter("@gold", character.Gold),
                            new MySqlParameter("@currentMapName", character.CurrentMapName),
                            new MySqlParameter("@currentPositionX", character.CurrentPosition.x),
                            new MySqlParameter("@currentPositionY", character.CurrentPosition.y),
                            new MySqlParameter("@currentPositionZ", character.CurrentPosition.z),
                            new MySqlParameter("@currentRotationX", character.CurrentRotation.x),
                            new MySqlParameter("@currentRotationY", character.CurrentRotation.y),
                            new MySqlParameter("@currentRotationZ", character.CurrentRotation.z),
                            new MySqlParameter("@respawnMapName", character.RespawnMapName),
                            new MySqlParameter("@respawnPositionX", character.RespawnPosition.x),
                            new MySqlParameter("@respawnPositionY", character.RespawnPosition.y),
                            new MySqlParameter("@respawnPositionZ", character.RespawnPosition.z),
                            new MySqlParameter("@mountDataId", character.MountDataId),
                            new MySqlParameter("@iconDataId", character.IconDataId),
                            new MySqlParameter("@frameDataId", character.FrameDataId),
                            new MySqlParameter("@titleDataId", character.TitleDataId));
                        await FillCharacterRelatesData(connection, transaction, character, null, null);
                        this.InvokeInstanceDevExtMethods("CreateCharacter", connection, transaction, userId, character);
                        await transaction.CommitAsync();
                    }
                    catch (System.Exception ex)
                    {
                        LogError(LogTag, "Transaction, Error occurs while create character: " + character.Id);
                        LogException(LogTag, ex);
                        await transaction.RollbackAsync();
                    }
                }
            }
        }

        private bool ReadCharacter(MySqlDataReader reader, out PlayerCharacterData result)
        {
            if (reader.Read())
            {
                result = new PlayerCharacterData();
                result.Id = reader.GetString(0);
                result.UserId = reader.GetString(1);
                result.DataId = reader.GetInt32(2);
                result.EntityId = reader.GetInt32(3);
                result.FactionId = reader.GetInt32(4);
                result.CharacterName = reader.GetString(5);
                result.Level = reader.GetInt32(6);
                result.Exp = reader.GetInt32(7);
                result.CurrentHp = reader.GetInt32(8);
                result.CurrentMp = reader.GetInt32(9);
                result.CurrentStamina = reader.GetInt32(10);
                result.CurrentFood = reader.GetInt32(11);
                result.CurrentWater = reader.GetInt32(12);
                result.EquipWeaponSet = reader.GetByte(13);
                result.StatPoint = reader.GetFloat(14);
                result.SkillPoint = reader.GetFloat(15);
                result.Gold = reader.GetInt32(16);
                result.PartyId = reader.GetInt32(17);
                result.GuildId = reader.GetInt32(18);
                result.GuildRole = reader.GetByte(19);
                result.SharedGuildExp = reader.GetInt32(20);
                result.CurrentMapName = reader.GetString(21);
                result.CurrentPosition = new Vec3(reader.GetFloat(22), reader.GetFloat(23), reader.GetFloat(24));
                result.CurrentRotation = new Vec3(reader.GetFloat(25), reader.GetFloat(26), reader.GetFloat(27));
                result.RespawnMapName = reader.GetString(28);
                result.RespawnPosition = new Vec3(reader.GetFloat(29), reader.GetFloat(30), reader.GetFloat(31));
                result.MountDataId = reader.GetInt32(32);
                result.IconDataId = reader.GetInt32(33);
                result.FrameDataId = reader.GetInt32(34);
                result.TitleDataId = reader.GetInt32(35);
                result.LastDeadTime = reader.GetInt64(36);
                result.UnmuteTime = reader.GetInt64(37);
                result.LastUpdate = ((System.DateTimeOffset)reader.GetDateTime(38)).ToUnixTimeSeconds();
                if (!reader.IsDBNull(39))
                    result.IsPkOn = reader.GetBoolean(39);
                if (!reader.IsDBNull(40))
                    result.LastPkOnTime = reader.GetInt64(40);
                if (!reader.IsDBNull(41))
                    result.PkPoint = reader.GetInt32(41);
                if (!reader.IsDBNull(42))
                    result.ConsecutivePkKills = reader.GetInt32(42);
                if (!reader.IsDBNull(43))
                    result.HighestPkPoint = reader.GetInt32(43);
                if (!reader.IsDBNull(44))
                    result.HighestConsecutivePkKills = reader.GetInt32(44);
                return true;
            }
            result = null;
            return false;
        }

        public override async UniTask<PlayerCharacterData> ReadCharacter(
            string id,
            bool withEquipWeapons = true,
            bool withAttributes = true,
            bool withSkills = true,
            bool withSkillUsages = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withSummons = true,
            bool withHotkeys = true,
            bool withQuests = true,
            bool withCurrencies = true,
            bool withServerCustomData = true,
            bool withPrivateCustomData = true,
            bool withPublicCustomData = true)
        {
            PlayerCharacterData result = null;
            await ExecuteReader((reader) =>
            {
                ReadCharacter(reader, out result);
            }, @"SELECT
                c.id, c.userId, c.dataId, c.entityId, c.factionId, c.characterName, c.level, c.exp,
                c.currentHp, c.currentMp, c.currentStamina, c.currentFood, c.currentWater,
                c.equipWeaponSet, c.statPoint, c.skillPoint, c.gold, c.partyId, c.guildId, c.guildRole, c.sharedGuildExp,
                c.currentMapName, c.currentPositionX, c.currentPositionY, c.currentPositionZ, c.currentRotationX, currentRotationY, currentRotationZ,
                c.respawnMapName, c.respawnPositionX, c.respawnPositionY, c.respawnPositionZ,
                c.mountDataId, c.iconDataId, c.frameDataId, c.titleDataId, c.lastDeadTime, c.unmuteTime, c.updateAt,
                cpk.isPkOn, cpk.lastPkOnTime, cpk.pkPoint, cpk.consecutivePkKills, cpk.highestPkPoint, cpk.highestConsecutivePkKills
                FROM characters AS c LEFT JOIN character_pk AS cpk ON c.id = cpk.id
                WHERE c.id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            // Found character, then read its relates data
            if (result != null)
            {
                List<EquipWeapons> selectableWeaponSets = new List<EquipWeapons>();
                List<CharacterAttribute> attributes = new List<CharacterAttribute>();
                List<CharacterSkill> skills = new List<CharacterSkill>();
                List<CharacterSkillUsage> skillUsages = new List<CharacterSkillUsage>();
                List<CharacterBuff> buffs = new List<CharacterBuff>();
                List<CharacterItem> equipItems = new List<CharacterItem>();
                List<CharacterItem> nonEquipItems = new List<CharacterItem>();
                List<CharacterSummon> summons = new List<CharacterSummon>();
                List<CharacterHotkey> hotkeys = new List<CharacterHotkey>();
                List<CharacterQuest> quests = new List<CharacterQuest>();
                List<CharacterCurrency> currencies = new List<CharacterCurrency>();

                List<CharacterDataBoolean> serverBools = new List<CharacterDataBoolean>();
                List<CharacterDataInt32> serverInts = new List<CharacterDataInt32>();
                List<CharacterDataFloat32> serverFloats = new List<CharacterDataFloat32>();

                List<CharacterDataBoolean> privateBools = new List<CharacterDataBoolean>();
                List<CharacterDataInt32> privateInts = new List<CharacterDataInt32>();
                List<CharacterDataFloat32> privateFloats = new List<CharacterDataFloat32>();

                List<CharacterDataBoolean> publicBools = new List<CharacterDataBoolean>();
                List<CharacterDataInt32> publicInts = new List<CharacterDataInt32>();
                List<CharacterDataFloat32> publicFloats = new List<CharacterDataFloat32>();

                // Read data
                List<UniTask> tasks = new List<UniTask>();
                if (withEquipWeapons)
                    tasks.Add(ReadCharacterEquipWeapons(id, selectableWeaponSets));
                if (withAttributes)
                    tasks.Add(ReadCharacterAttributes(id, attributes));
                if (withSkills)
                    tasks.Add(ReadCharacterSkills(id, skills));
                if (withSkillUsages)
                    tasks.Add(ReadCharacterSkillUsages(id, skillUsages));
                if (withBuffs)
                    tasks.Add(ReadCharacterBuffs(id, buffs));
                if (withEquipItems)
                    tasks.Add(ReadCharacterEquipItems(id, equipItems));
                if (withNonEquipItems)
                    tasks.Add(ReadCharacterNonEquipItems(id, nonEquipItems));
                if (withSummons)
                    tasks.Add(ReadCharacterSummons(id, summons));
                if (withHotkeys)
                    tasks.Add(ReadCharacterHotkeys(id, hotkeys));
                if (withQuests)
                    tasks.Add(ReadCharacterQuests(id, quests));
                if (withCurrencies)
                    tasks.Add(ReadCharacterCurrencies(id, currencies));
                if (withServerCustomData)
                {
                    tasks.Add(ReadCharacterDataBooleans("character_server_boolean", id, serverBools));
                    tasks.Add(ReadCharacterDataInt32s("character_server_int32", id, serverInts));
                    tasks.Add(ReadCharacterDataFloat32s("character_server_float32", id, serverFloats));
                }
                if (withPrivateCustomData)
                {
                    tasks.Add(ReadCharacterDataBooleans("character_private_boolean", id, privateBools));
                    tasks.Add(ReadCharacterDataInt32s("character_private_int32", id, privateInts));
                    tasks.Add(ReadCharacterDataFloat32s("character_private_float32", id, privateFloats));
                }
                if (withPublicCustomData)
                {
                    tasks.Add(ReadCharacterDataBooleans("character_public_boolean", id, publicBools));
                    tasks.Add(ReadCharacterDataInt32s("character_public_int32", id, publicInts));
                    tasks.Add(ReadCharacterDataFloat32s("character_public_float32", id, publicFloats));
                }
                await UniTask.WhenAll(tasks);
                // Assign read data
                if (withEquipWeapons)
                    result.SelectableWeaponSets = selectableWeaponSets;
                if (withAttributes)
                    result.Attributes = attributes;
                if (withSkills)
                    result.Skills = skills;
                if (withSkillUsages)
                    result.SkillUsages = skillUsages;
                if (withBuffs)
                    result.Buffs = buffs;
                if (withEquipItems)
                    result.EquipItems = equipItems;
                if (withNonEquipItems)
                    result.NonEquipItems = nonEquipItems;
                if (withSummons)
                    result.Summons = summons;
                if (withHotkeys)
                    result.Hotkeys = hotkeys;
                if (withQuests)
                    result.Quests = quests;
                if (withCurrencies)
                    result.Currencies = currencies;
                if (withServerCustomData)
                {
                    result.ServerBools = serverBools;
                    result.ServerInts = serverInts;
                    result.ServerFloats = serverFloats;
                }
                if (withPrivateCustomData)
                {
                    result.PrivateBools = privateBools;
                    result.PrivateInts = privateInts;
                    result.PrivateFloats = privateFloats;
                }
                if (withPublicCustomData)
                {
                    result.PublicBools = publicBools;
                    result.PublicInts = publicInts;
                    result.PublicFloats = publicFloats;
                }
                // Invoke dev extension methods
                this.InvokeInstanceDevExtMethods("ReadCharacter",
                    result,
                    withEquipWeapons,
                    withAttributes,
                    withSkills,
                    withSkillUsages,
                    withBuffs,
                    withEquipItems,
                    withNonEquipItems,
                    withSummons,
                    withHotkeys,
                    withQuests,
                    withCurrencies,
                    withServerCustomData,
                    withPrivateCustomData,
                    withPublicCustomData);
            }
            return result;
        }

        public override async UniTask<List<PlayerCharacterData>> ReadCharacters(string userId)
        {
            List<PlayerCharacterData> result = new List<PlayerCharacterData>();
            List<string> characterIds = new List<string>();
            await ExecuteReader((reader) =>
            {
                while (reader.Read())
                {
                    characterIds.Add(reader.GetString(0));
                }
            }, "SELECT id FROM characters WHERE userId=@userId ORDER BY updateAt DESC", new MySqlParameter("@userId", userId));
            foreach (string characterId in characterIds)
            {
                result.Add(await ReadCharacter(characterId, true, false, false, false, false, true, false, false, false, false, false, false, false, true));
            }
            return result;
        }

        public override async UniTask UpdateCharacter(IPlayerCharacterData character, List<CharacterBuff> summonBuffs, List<CharacterItem> storageItems, bool deleteStorageReservation)
        {
            using (MySqlConnection connection = NewConnection())
            {
                await OpenConnection(connection);
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await ExecuteNonQuery(connection, transaction, @"INSERT INTO character_pk
                    (id, isPkOn, lastPkOnTime, pkPoint, consecutivePkKills, highestPkPoint, highestConsecutivePkKills) VALUES
                    (@id, @isPkOn, @lastPkOnTime, @pkPoint, @consecutivePkKills, @highestPkPoint, @highestConsecutivePkKills)
                    ON DUPLICATE KEY UPDATE
                    isPkOn = @isPkOn,
                    lastPkOnTime = @lastPkOnTime,
                    pkPoint = @pkPoint,
                    consecutivePkKills = @consecutivePkKills,
                    highestPkPoint = @highestPkPoint,
                    highestConsecutivePkKills = @highestConsecutivePkKills",
                            new MySqlParameter("@id", character.Id),
                            new MySqlParameter("@isPkOn", character.IsPkOn),
                            new MySqlParameter("@lastPkOnTime", character.LastPkOnTime),
                            new MySqlParameter("@pkPoint", character.PkPoint),
                            new MySqlParameter("@consecutivePkKills", character.ConsecutivePkKills),
                            new MySqlParameter("@highestPkPoint", character.HighestPkPoint),
                            new MySqlParameter("@highestConsecutivePkKills", character.HighestConsecutivePkKills));
                        await ExecuteNonQuery(connection, transaction, @"UPDATE characters SET
                    dataId=@dataId,
                    entityId=@entityId,
                    factionId=@factionId,
                    characterName=@characterName,
                    level=@level,
                    exp=@exp,
                    currentHp=@currentHp,
                    currentMp=@currentMp,
                    currentStamina=@currentStamina,
                    currentFood=@currentFood,
                    currentWater=@currentWater,
                    equipWeaponSet=@equipWeaponSet,
                    statPoint=@statPoint,
                    skillPoint=@skillPoint,
                    gold=@gold,
                    currentMapName=@currentMapName,
                    currentPositionX=@currentPositionX,
                    currentPositionY=@currentPositionY,
                    currentPositionZ=@currentPositionZ,
                    currentRotationX=@currentRotationX,
                    currentRotationY=@currentRotationY,
                    currentRotationZ=@currentRotationZ,
                    respawnMapName=@respawnMapName,
                    respawnPositionX=@respawnPositionX,
                    respawnPositionY=@respawnPositionY,
                    respawnPositionZ=@respawnPositionZ,
                    mountDataId=@mountDataId,
                    iconDataId=@iconDataId,
                    frameDataId=@frameDataId,
                    titleDataId=@titleDataId,
                    lastDeadTime=@lastDeadTime,
                    unmuteTime=@unmuteTime
                    WHERE id=@id",
                            new MySqlParameter("@dataId", character.DataId),
                            new MySqlParameter("@entityId", character.EntityId),
                            new MySqlParameter("@factionId", character.FactionId),
                            new MySqlParameter("@characterName", character.CharacterName),
                            new MySqlParameter("@level", character.Level),
                            new MySqlParameter("@exp", character.Exp),
                            new MySqlParameter("@currentHp", character.CurrentHp),
                            new MySqlParameter("@currentMp", character.CurrentMp),
                            new MySqlParameter("@currentStamina", character.CurrentStamina),
                            new MySqlParameter("@currentFood", character.CurrentFood),
                            new MySqlParameter("@currentWater", character.CurrentWater),
                            new MySqlParameter("@equipWeaponSet", character.EquipWeaponSet),
                            new MySqlParameter("@statPoint", character.StatPoint),
                            new MySqlParameter("@skillPoint", character.SkillPoint),
                            new MySqlParameter("@gold", character.Gold),
                            new MySqlParameter("@currentMapName", character.CurrentMapName),
                            new MySqlParameter("@currentPositionX", character.CurrentPosition.x),
                            new MySqlParameter("@currentPositionY", character.CurrentPosition.y),
                            new MySqlParameter("@currentPositionZ", character.CurrentPosition.z),
                            new MySqlParameter("@currentRotationX", character.CurrentRotation.x),
                            new MySqlParameter("@currentRotationY", character.CurrentRotation.y),
                            new MySqlParameter("@currentRotationZ", character.CurrentRotation.z),
                            new MySqlParameter("@respawnMapName", character.RespawnMapName),
                            new MySqlParameter("@respawnPositionX", character.RespawnPosition.x),
                            new MySqlParameter("@respawnPositionY", character.RespawnPosition.y),
                            new MySqlParameter("@respawnPositionZ", character.RespawnPosition.z),
                            new MySqlParameter("@mountDataId", character.MountDataId),
                            new MySqlParameter("@iconDataId", character.IconDataId),
                            new MySqlParameter("@frameDataId", character.FrameDataId),
                            new MySqlParameter("@titleDataId", character.TitleDataId),
                            new MySqlParameter("@lastDeadTime", character.LastDeadTime),
                            new MySqlParameter("@unmuteTime", character.UnmuteTime),
                            new MySqlParameter("@id", character.Id));
                        await FillCharacterRelatesData(connection, transaction, character, summonBuffs, storageItems);
                        if (deleteStorageReservation)
                        {
                            await ExecuteNonQuery(connection, transaction, "DELETE FROM storage_reservation WHERE reserverId=@reserverId",
                                new MySqlParameter("@reserverId", character.Id));
                        }
                        this.InvokeInstanceDevExtMethods("UpdateCharacter", connection, transaction, character);
                        await transaction.CommitAsync();
                    }
                    catch (System.Exception ex)
                    {
                        LogError(LogTag, "Transaction, Error occurs while update character: " + character.Id);
                        LogException(LogTag, ex);
                        await transaction.RollbackAsync();
                    }
                }
            }
        }

        public override async UniTask DeleteCharacter(string userId, string id)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM characters WHERE id=@id AND userId=@userId",
                new MySqlParameter("@id", id),
                new MySqlParameter("@userId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                using (MySqlConnection connection = NewConnection())
                {
                    await OpenConnection(connection);
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            await ExecuteNonQuery(connection, transaction, "DELETE FROM characters WHERE id=@characterId", new MySqlParameter("@characterId", id));
                            await ExecuteNonQuery(connection, transaction, "DELETE FROM character_pk WHERE id=@characterId", new MySqlParameter("@characterId", id));
                            await ExecuteNonQuery(connection, transaction, "DELETE FROM friend WHERE characterId1 LIKE @characterId OR characterId2 LIKE @characterId", new MySqlParameter("@characterId", id));
                            await DeleteCharacterAttributes(connection, transaction, id);
                            await DeleteCharacterCurrencies(connection, transaction, id);
                            await DeleteCharacterBuffs(connection, transaction, id);
                            await DeleteCharacterHotkeys(connection, transaction, id);
                            await DeleteCharacterItems(connection, transaction, id);
                            await DeleteCharacterQuests(connection, transaction, id);
                            await DeleteCharacterSkills(connection, transaction, id);
                            await DeleteCharacterSkillUsages(connection, transaction, id);
                            await DeleteCharacterSummons(connection, transaction, id);

                            await DeleteCharacterDataBooleans(connection, transaction, "character_server_boolean", id);
                            await DeleteCharacterDataInt32s(connection, transaction, "character_server_int32", id);
                            await DeleteCharacterDataFloat32s(connection, transaction, "character_server_float32", id);

                            await DeleteCharacterDataBooleans(connection, transaction, "character_private_boolean", id);
                            await DeleteCharacterDataInt32s(connection, transaction, "character_private_int32", id);
                            await DeleteCharacterDataFloat32s(connection, transaction, "character_private_float32", id);

                            await DeleteCharacterDataBooleans(connection, transaction, "character_public_boolean", id);
                            await DeleteCharacterDataInt32s(connection, transaction, "character_public_int32", id);
                            await DeleteCharacterDataFloat32s(connection, transaction, "character_public_float32", id);

                            this.InvokeInstanceDevExtMethods("DeleteCharacter", connection, transaction, userId, id);
                            await transaction.CommitAsync();
                        }
                        catch (System.Exception ex)
                        {
                            LogError(LogTag, "Transaction, Error occurs while deleting character: " + id);
                            LogException(LogTag, ex);
                            await transaction.RollbackAsync();
                        }
                    }
                }
            }
        }

        public override async UniTask<long> FindCharacterName(string characterName)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM characters WHERE characterName LIKE @characterName",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (long)result : 0;
        }

        public override async UniTask<string> GetIdByCharacterName(string characterName)
        {
            object result = await ExecuteScalar("SELECT id FROM characters WHERE characterName LIKE @characterName LIMIT 1",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public override async UniTask<string> GetUserIdByCharacterName(string characterName)
        {
            object result = await ExecuteScalar("SELECT userId FROM characters WHERE characterName LIKE @characterName LIMIT 1",
                new MySqlParameter("@characterName", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public override async UniTask<List<SocialCharacterData>> FindCharacters(string finderId, string characterName, int skip, int limit)
        {
            string excludeIdsQuery = "(id!='" + finderId + "'";
            // Exclude friend, requested characters
            await ExecuteReader((reader) =>
            {
                while (reader.Read())
                {
                    excludeIdsQuery += " AND id!='" + reader.GetString(0) + "'";
                }
            }, "SELECT characterId2 FROM friend WHERE characterId1='" + finderId + "'");
            excludeIdsQuery += ")";
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            await ExecuteReader((reader) =>
            {
                SocialCharacterData socialCharacterData;
                while (reader.Read())
                {
                    // Get some required data, other data will be set at server side
                    socialCharacterData = new SocialCharacterData();
                    socialCharacterData.id = reader.GetString(0);
                    socialCharacterData.dataId = reader.GetInt32(1);
                    socialCharacterData.characterName = reader.GetString(2);
                    socialCharacterData.level = reader.GetInt32(3);
                    result.Add(socialCharacterData);
                }
            }, "SELECT id, dataId, characterName, level FROM characters WHERE characterName LIKE @characterName AND " + excludeIdsQuery + " ORDER BY RAND() LIMIT " + skip + ", " + limit,
                new MySqlParameter("@characterName", "%" + characterName + "%"));
            return result;
        }

        public override async UniTask CreateFriend(string id1, string id2, byte state)
        {
            using (MySqlConnection connection = NewConnection())
            {
                await OpenConnection(connection);
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await ExecuteNonQuery(connection, transaction, "DELETE FROM friend WHERE " +
                           "characterId1 LIKE @characterId1 AND " +
                           "characterId2 LIKE @characterId2",
                           new MySqlParameter("@characterId1", id1),
                           new MySqlParameter("@characterId2", id2));
                        await ExecuteNonQuery(connection, transaction, "INSERT INTO friend " +
                            "(characterId1, characterId2, state) VALUES " +
                            "(@characterId1, @characterId2, @state)",
                            new MySqlParameter("@characterId1", id1),
                            new MySqlParameter("@characterId2", id2),
                            new MySqlParameter("@state", state));
                        await transaction.CommitAsync();
                    }
                    catch (System.Exception ex)
                    {
                        LogError(LogTag, "Transaction, Error occurs while creating friend: " + id1 + " " + id2);
                        LogException(LogTag, ex);
                        await transaction.RollbackAsync();
                    }
                }
            }
        }

        public override async UniTask DeleteFriend(string id1, string id2)
        {
            await ExecuteNonQuery("DELETE FROM friend WHERE " +
               "characterId1 LIKE @characterId1 AND " +
               "characterId2 LIKE @characterId2",
               new MySqlParameter("@characterId1", id1),
               new MySqlParameter("@characterId2", id2));
        }

        public override async UniTask<List<SocialCharacterData>> ReadFriends(string id, bool readById2, byte state, int skip, int limit)
        {
            List<SocialCharacterData> result = new List<SocialCharacterData>();
            List<string> characterIds = new List<string>();
            if (readById2)
            {
                await ExecuteReader((reader) =>
                {
                    while (reader.Read())
                    {
                        characterIds.Add(reader.GetString(0));
                    }
                }, "SELECT characterId1 FROM friend WHERE characterId2=@id AND state=" + state + " LIMIT " + skip + ", " + limit,
                    new MySqlParameter("@id", id));
            }
            else
            {
                await ExecuteReader((reader) =>
                {
                    while (reader.Read())
                    {
                        characterIds.Add(reader.GetString(0));
                    }
                }, "SELECT characterId2 FROM friend WHERE characterId1=@id AND state=" + state + " LIMIT " + skip + ", " + limit,
                    new MySqlParameter("@id", id));
            }
            SocialCharacterData socialCharacterData;
            foreach (string characterId in characterIds)
            {
                await ExecuteReader((reader) =>
                {
                    while (reader.Read())
                    {
                        // Get some required data, other data will be set at server side
                        socialCharacterData = new SocialCharacterData();
                        socialCharacterData.id = reader.GetString(0);
                        socialCharacterData.dataId = reader.GetInt32(1);
                        socialCharacterData.characterName = reader.GetString(2);
                        socialCharacterData.level = reader.GetInt32(3);
                        result.Add(socialCharacterData);
                    }
                }, "SELECT id, dataId, characterName, level FROM characters WHERE BINARY id = @id",
                    new MySqlParameter("@id", characterId));
            }
            return result;
        }

        public override async UniTask<int> GetFriendRequestNotification(string characterId)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM friend WHERE characterId2=@characterId AND state=1",
                new MySqlParameter("@characterId", characterId));
            return (int)(long)result;
        }
    }
}
#endif