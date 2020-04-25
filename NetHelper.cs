using System;
using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using MagicStoragePlus.Components;

namespace MagicStoragePlus
{
    public static class NetHelper
    {
        enum MessageType : byte
        {
            SearchAndRefreshNetwork,
            TryStorageOp,
            StorageOperationResult,
            RefreshNetworkItems,
            ClientSendTEUpdate,
            ResetCompactStage
        }

        public enum StorageOp : byte
        {
            Deposit,
            DepositList,
            Withdraw,
            WithdrawToInventory,
            WithdrawJustRemove
        }

        static bool queueUpdates = false;
        static Queue<int> updateQueue = new Queue<int>();
        static HashSet<int> updateQueueContains = new HashSet<int>();

        public static void Unload()
        {
            updateQueue.Clear();
            updateQueue = null;

            updateQueueContains.Clear();
            updateQueueContains = null;
        }

        public static void HandlePacket(BinaryReader reader, int sender)
        {
            MessageType type = (MessageType)reader.ReadByte();
            if (type == MessageType.SearchAndRefreshNetwork)
                ReceiveSearchAndRefresh(reader);
            else if (type == MessageType.TryStorageOp)
                ReceiveStorageOp(reader, sender);
            else if (type == MessageType.StorageOperationResult)
                ReceiveOpResult(reader);
            else if (type == MessageType.RefreshNetworkItems)
                MagicStoragePlus.Instance.StorageUI.RefreshItems();
            else if (type == MessageType.ClientSendTEUpdate)
                ReceiveClientSendTEUpdate(reader, sender);
            /* else if (type == MessageType.TryStationOperation)
                 ReceiveStationOperation(reader, sender);
             else if (type == MessageType.StationOperationResult)
                 ReceiveStationResult(reader);
                 */
            else if (type == MessageType.ResetCompactStage)
                ReceiveResetCompactStage(reader);
            /*
             * else if (type == MessageType.CraftRequest)
                ReceiveCraftRequest(reader, sender);
            else if (type == MessageType.CraftResult)
                ReceiveCraftResult(reader);
            */
        }

        public static void SendComponentPlace(int i, int j, int type)
        {
            if (Main.netMode == 1)
            {
                NetMessage.SendTileRange(Main.myPlayer, i, j, 2, 2);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, type);
            }
        }

        public static void StartUpdateQueue()
        {
            queueUpdates = true;
        }

        public static void SendTEUpdate(int id, Point16 position)
        {
            if (Main.netMode != 2)
                return;

            if (queueUpdates)
            {
                if (!updateQueueContains.Contains(id))
                {
                    updateQueue.Enqueue(id);
                    updateQueueContains.Add(id);
                }
            }
            else
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, id, position.X, position.Y);
            }
        }

        public static void ProcessUpdateQueue()
        {
            if (queueUpdates)
            {
                queueUpdates = false;
                while (updateQueue.Count > 0)
                {
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, updateQueue.Dequeue());
                }
                updateQueueContains.Clear();
            }
        }

        public static void SendSearchAndRefresh(int i, int j)
        {
            if (Main.netMode == 1)
            {
                ModPacket packet = MagicStoragePlus.Instance.GetPacket();
                packet.Write((byte)MessageType.SearchAndRefreshNetwork);
                packet.Write((short)i);
                packet.Write((short)j);
                packet.Send();
            }
        }

        static void ReceiveSearchAndRefresh(BinaryReader reader)
        {
            Point16 point = new Point16(reader.ReadInt16(), reader.ReadInt16());
            TEStorageComponent.SearchAndRefreshNetwork(point);
        }

        static ModPacket PrepareStorageOperation(int ent, StorageOp op)
        {
            ModPacket packet = MagicStoragePlus.Instance.GetPacket();
            packet.Write((byte)MessageType.TryStorageOp);
            packet.Write(ent);
            packet.Write((byte)op);
            return packet;
        }

        static ModPacket PrepareOpResult(StorageOp op)
        {
            ModPacket packet = MagicStoragePlus.Instance.GetPacket();
            packet.Write((byte)MessageType.StorageOperationResult);
            packet.Write((byte)op);
            return packet;
        }

        public static void SendDeposit(int ent, Item item)
        {
            if (Main.netMode == 1)
            {
                ModPacket packet = PrepareStorageOperation(ent, StorageOp.Deposit);
                ItemIO.Send(item, packet, true);
                packet.Send();
            }
        }

        public static void SendWithdraw(int ent, Item item, StorageOp type)
        {
            if (Main.netMode == 1)
            {
                ModPacket packet = PrepareStorageOperation(ent, type);
                ItemIO.Send(item, packet, true);
                packet.Send();
            }
        }

        public static void SendDepositAll(int ent, List<Item> items)
        {
            if (Main.netMode == 1)
            {
                ModPacket packet = PrepareStorageOperation(ent, StorageOp.DepositList);
                packet.Write((byte)items.Count);
                foreach (Item item in items)
                    ItemIO.Send(item, packet, true);
                packet.Send();
            }
        }

        public static void ReceiveStorageOp(BinaryReader reader, int sender)
        {
            if (Main.netMode != 2)
                return;

            int ent = reader.ReadInt32();
            if (!TileEntity.ByID.ContainsKey(ent) || !(TileEntity.ByID[ent] is TEStorageHeart))
                return;

            TEStorageHeart heart = (TEStorageHeart)TileEntity.ByID[ent];
            var op = (StorageOp)reader.ReadByte();
            if (op == StorageOp.Deposit)
            {
                Item item = ItemIO.Receive(reader, true);
                heart.DepositItem(item);
                if (!item.IsAir)
                {
                    ModPacket packet = PrepareOpResult(op);
                    ItemIO.Send(item, packet, true);
                    packet.Send(sender);
                }
            }
            else if (op == StorageOp.DepositList)
            {
                int count = reader.ReadByte();
                List<Item> items = new List<Item>();
                StartUpdateQueue();
                for (int k = 0; k < count; k++)
                {
                    Item item = ItemIO.Receive(reader, true);
                    heart.DepositItem(item);
                    if (!item.IsAir)
                        items.Add(item);
                }
                ProcessUpdateQueue();
                if (items.Count > 0)
                {
                    ModPacket packet = PrepareOpResult(op);
                    packet.Write((byte)items.Count);
                    foreach (Item item in items)
                        ItemIO.Send(item, packet, true);
                    packet.Send(sender);
                }
            }
            else if (op == StorageOp.Withdraw || op == StorageOp.WithdrawToInventory)
            {
                Item item = ItemIO.Receive(reader, true);
                item = heart.TryWithdraw(item);
                if (!item.IsAir)
                {
                    ModPacket packet = PrepareOpResult(op);
                    ItemIO.Send(item, packet, true);
                    packet.Send(sender);
                }
            }
            else if (op == StorageOp.WithdrawJustRemove)
            {
                // @Robustness: We assume that nothing could go wrong here.
                // And maybe nothing could. But still something to look at for potential bugs...
                Item item = ItemIO.Receive(reader, true);
                heart.TryWithdraw(item);
            }

            SendRefreshNetworkItems(ent);
        }

        public static void ReceiveOpResult(BinaryReader reader)
        {
            if (Main.netMode != 1)
                return;

            var op = (StorageOp)reader.ReadByte();
            if (op == StorageOp.Deposit || op == StorageOp.Withdraw || op == StorageOp.WithdrawToInventory)
            {
                Item item = ItemIO.Receive(reader, true);
                StoragePlayer.GetItem(item, op != StorageOp.WithdrawToInventory);
            }
            else if (op == StorageOp.DepositList)
            {
                int count = reader.ReadByte();
                for (int k = 0; k < count; k++)
                {
                    Item item = ItemIO.Receive(reader, true);
                    StoragePlayer.GetItem(item, false);
                }
            }
        }

        public static void SendRefreshNetworkItems(int ent)
        {
            if (Main.netMode == 2)
            {
                ModPacket packet = MagicStoragePlus.Instance.GetPacket();
                packet.Write((byte)MessageType.RefreshNetworkItems);
                packet.Write(ent);
                packet.Send();
            }
        }

        public static void ClientSendTEUpdate(int id)
        {
            if (Main.netMode == 1)
            {
                ModPacket packet = MagicStoragePlus.Instance.GetPacket();
                packet.Write((byte)MessageType.ClientSendTEUpdate);
                packet.Write(id);
                TileEntity.Write(packet, TileEntity.ByID[id], true);
                packet.Send();
            }
        }

        public static void ReceiveClientSendTEUpdate(BinaryReader reader, int sender)
        {
            if (Main.netMode == 2)
            {
                int id = reader.ReadInt32();
                TileEntity ent = TileEntity.Read(reader, true);
                ent.ID = id;
                TileEntity.ByID[id] = ent;
                TileEntity.ByPosition[ent.Position] = ent;
                if (ent is TEStorageUnit)
                {
                    TEStorageHeart heart = ((TEStorageUnit)ent).GetHeart();
                    if (heart != null)
                        heart.ResetCompactStage();
                }
                NetMessage.SendData(MessageID.TileEntitySharing, -1, sender, null, id, ent.Position.X, ent.Position.Y);
            }
        }

        public static void SendResetCompactStage(int ent)
        {
            if (Main.netMode == 1)
            {
                ModPacket packet = MagicStoragePlus.Instance.GetPacket();
                packet.Write((byte)MessageType.ResetCompactStage);
                packet.Write(ent);
                packet.Send();
            }
        }

        public static void ReceiveResetCompactStage(BinaryReader reader)
        {
            if (Main.netMode == 2)
            {
                int ent = reader.ReadInt32();
                if (TileEntity.ByID[ent] is TEStorageHeart)
                {
                    ((TEStorageHeart)TileEntity.ByID[ent]).ResetCompactStage();
                }
            }
        }
    }
}