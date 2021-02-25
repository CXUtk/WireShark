using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace WireShark {
    public static class WiringWarpper {

        private static WireAccelerator _wireAccelerator;

        public static WireAccelerator GetWireAccelerator() {
            return _wireAccelerator;
        }

        // Token: 0x06000753 RID: 1875 RVA: 0x0035517C File Offset: 0x0035337C
        public static void SetCurrentUser(int plr = -1) {
            if (plr < 0 || plr >= 255) {
                plr = 254;
            }
            if (Main.netMode == NetmodeID.SinglePlayer) {
                plr = Main.myPlayer;
            }
            CurrentUser = plr;
        }

        // Token: 0x06000754 RID: 1876 RVA: 0x003551A8 File Offset: 0x003533A8
        public static void Initialize() {
            _wireAccelerator = new WireAccelerator();
            _wireSkip = new Dictionary<Point16, bool>();
            _wireList = new DoubleStack<Point16>(1024, 0);
            _wireDirectionList = new DoubleStack<byte>(1024, 0);
            _toProcess = new Dictionary<Point16, byte>();
            _GatesCurrent = new Queue<Point16>();
            _GatesNext = new Queue<Point16>();
            _GatesDone = new Dictionary<Point16, bool>();
            _LampsToCheck = new Queue<Point16>();
            _PixelBoxTriggers = new Dictionary<Point16, byte>();
            _inPumpX = new int[20];
            _inPumpY = new int[20];
            _outPumpX = new int[20];
            _outPumpY = new int[20];
            _teleport = new Vector2[2];
            _mechX = new int[1000];
            _mechY = new int[1000];
            _mechTime = new int[1000];
        }

        // Token: 0x06000755 RID: 1877 RVA: 0x00355283 File Offset: 0x00353483
        public static void SkipWire(int x, int y) {
            _wireSkip[new Point16(x, y)] = true;
        }

        // Token: 0x06000756 RID: 1878 RVA: 0x00355297 File Offset: 0x00353497
        public static void SkipWire(Point16 point) {
            _wireSkip[point] = true;
        }

        // Token: 0x06000757 RID: 1879 RVA: 0x003552A8 File Offset: 0x003534A8

        // Mech 应该就是可以激活的计时器
        public static void UpdateMech() {
            SetCurrentUser(-1);
            for (int i = _numMechs - 1; i >= 0; i--) {
                _mechTime[i]--;
                if (Main.tile[_mechX[i], _mechY[i]].active() && Main.tile[_mechX[i], _mechY[i]].type == 144) {
                    if (Main.tile[_mechX[i], _mechY[i]].frameY == 0) {
                        _mechTime[i] = 0;
                    } else {
                        int num = (int)(Main.tile[_mechX[i], _mechY[i]].frameX / 18);
                        if (num == 0) {
                            num = 60;
                        } else if (num == 1) {
                            num = 180;
                        } else if (num == 2) {
                            num = 300;
                        }
                        if (Math.IEEERemainder((double)_mechTime[i], (double)num) == 0.0) {
                            _mechTime[i] = 18000;
                            BigTripWire(_mechX[i], _mechY[i], 1, 1);
                        }
                    }
                }
                if (_mechTime[i] <= 0) {
                    if (Main.tile[_mechX[i], _mechY[i]].active() && Main.tile[_mechX[i], _mechY[i]].type == 144) {
                        Main.tile[_mechX[i], _mechY[i]].frameY = 0;
                        NetMessage.SendTileSquare(-1, _mechX[i], _mechY[i], 1, TileChangeType.None);
                    }
                    if (Main.tile[_mechX[i], _mechY[i]].active() && Main.tile[_mechX[i], _mechY[i]].type == 411) {
                        Tile tile = Main.tile[_mechX[i], _mechY[i]];
                        int num2 = (int)(tile.frameX % 36 / 18);
                        int num3 = (int)(tile.frameY % 36 / 18);
                        int num4 = _mechX[i] - num2;
                        int num5 = _mechY[i] - num3;
                        int num6 = 36;
                        if (Main.tile[num4, num5].frameX >= 36) {
                            num6 = -36;
                        }
                        for (int j = num4; j < num4 + 2; j++) {
                            for (int k = num5; k < num5 + 2; k++) {
                                Main.tile[j, k].frameX = (short)((int)Main.tile[j, k].frameX + num6);
                            }
                        }
                        NetMessage.SendTileSquare(-1, num4, num5, 2, TileChangeType.None);
                    }
                    for (int l = i; l < _numMechs; l++) {
                        _mechX[l] = _mechX[l + 1];
                        _mechY[l] = _mechY[l + 1];
                        _mechTime[l] = _mechTime[l + 1];
                    }
                    _numMechs--;
                }
            }
        }

        // Token: 0x06000758 RID: 1880 RVA: 0x003555B8 File Offset: 0x003537B8
        public static void HitSwitch(int i, int j) {
            if (!WorldGen.InWorld(i, j, 0)) {
                return;
            }
            if (Main.tile[i, j] == null) {
                return;
            }
            if (Main.tile[i, j].type == 135 || Main.tile[i, j].type == 314 || Main.tile[i, j].type == 423 || Main.tile[i, j].type == 428 || Main.tile[i, j].type == 442) {
                Main.PlaySound(SoundID.Mech, i * 16, j * 16, 0, 1f, 0f);
                BigTripWire(i, j, 1, 1);
                return;
            }
            if (Main.tile[i, j].type == 440) {
                Main.PlaySound(SoundID.Mech, i * 16 + 16, j * 16 + 16, 0, 1f, 0f);
                BigTripWire(i, j, 3, 3);
                return;
            }
            if (Main.tile[i, j].type == 136) {
                if (Main.tile[i, j].frameY == 0) {
                    Main.tile[i, j].frameY = 18;
                } else {
                    Main.tile[i, j].frameY = 0;
                }
                Main.PlaySound(SoundID.Mech, i * 16, j * 16, 0, 1f, 0f);
                BigTripWire(i, j, 1, 1);
                return;
            }
            if (Main.tile[i, j].type == 144) {
                if (Main.tile[i, j].frameY == 0) {
                    Main.tile[i, j].frameY = 18;
                    if (Main.netMode != NetmodeID.MultiplayerClient) {
                        CheckMech(i, j, 18000);
                    }
                } else {
                    Main.tile[i, j].frameY = 0;
                }
                Main.PlaySound(SoundID.Mech, i * 16, j * 16, 0, 1f, 0f);
                return;
            }
            if (Main.tile[i, j].type == 441 || Main.tile[i, j].type == 468) {
                int num = (int)(Main.tile[i, j].frameX / 18 * -1);
                int num2 = (int)(Main.tile[i, j].frameY / 18 * -1);
                num %= 4;
                if (num < -1) {
                    num += 2;
                }
                num += i;
                num2 += j;
                Main.PlaySound(SoundID.Mech, i * 16, j * 16, 0, 1f, 0f);
                BigTripWire(num, num2, 2, 2);
                return;
            }
            if (Main.tile[i, j].type == 132 || Main.tile[i, j].type == 411) {
                short num3 = 36;
                int num4 = (int)(Main.tile[i, j].frameX / 18 * -1);
                int num5 = (int)(Main.tile[i, j].frameY / 18 * -1);
                num4 %= 4;
                if (num4 < -1) {
                    num4 += 2;
                    num3 = -36;
                }
                num4 += i;
                num5 += j;
                if (Main.netMode != NetmodeID.MultiplayerClient && Main.tile[num4, num5].type == 411) {
                    CheckMech(num4, num5, 60);
                }
                for (int k = num4; k < num4 + 2; k++) {
                    for (int l = num5; l < num5 + 2; l++) {
                        if (Main.tile[k, l].type == 132 || Main.tile[k, l].type == 411) {
                            Tile tile = Main.tile[k, l];
                            tile.frameX += num3;
                        }
                    }
                }
                WorldGen.TileFrame(num4, num5, false, false);
                Main.PlaySound(SoundID.Mech, i * 16, j * 16, 0, 1f, 0f);
                BigTripWire(num4, num5, 2, 2);
            }
        }

        // Token: 0x06000759 RID: 1881 RVA: 0x0035599F File Offset: 0x00353B9F
        public static void PokeLogicGate(int lampX, int lampY) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                return;
            }
            _LampsToCheck.Enqueue(new Point16(lampX, lampY));
            LogicGatePass();
        }

        // Token: 0x0600075A RID: 1882 RVA: 0x003559C0 File Offset: 0x00353BC0
        public static bool Actuate(int i, int j) {
            Tile tile = Main.tile[i, j];
            if (!tile.actuator()) {
                return false;
            }
            if ((tile.type != 226 || (double)j <= Main.worldSurface || NPC.downedPlantBoss) && ((double)j <= Main.worldSurface || NPC.downedGolemBoss || Main.tile[i, j - 1].type != 237)) {
                if (tile.inActive()) {
                    ReActive(i, j);
                } else {
                    DeActive(i, j);
                }
            }
            return true;
        }

        // Token: 0x0600075B RID: 1883 RVA: 0x00355A44 File Offset: 0x00353C44
        public static void ActuateForced(int i, int j) {
            Tile tile = Main.tile[i, j];
            if (tile.type == 226 && (double)j > Main.worldSurface && !NPC.downedPlantBoss) {
                return;
            }
            if (tile.inActive()) {
                ReActive(i, j);
                return;
            }
            DeActive(i, j);
        }

        // Token: 0x0600075C RID: 1884 RVA: 0x00355A94 File Offset: 0x00353C94
        public static void MassWireOperation(Point ps, Point pe, Player master) {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < 58; i++) {
                if (master.inventory[i].type == ItemID.Wire) {
                    num += master.inventory[i].stack;
                }
                if (master.inventory[i].type == ItemID.Actuator) {
                    num2 += master.inventory[i].stack;
                }
            }
            int num6 = num;
            int num3 = num2;
            MassWireOperationInner(ps, pe, master.Center, master.direction == 1, ref num, ref num2);
            int num4 = num6 - num;
            int num5 = num3 - num2;
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.MassWireOperationPay, master.whoAmI, -1, null, 530, (float)num4, (float)master.whoAmI, 0f, 0, 0, 0);
                NetMessage.SendData(MessageID.MassWireOperationPay, master.whoAmI, -1, null, 849, (float)num5, (float)master.whoAmI, 0f, 0, 0, 0);
                return;
            }
            for (int j = 0; j < num4; j++) {
                master.ConsumeItem(530, false);
            }
            for (int k = 0; k < num5; k++) {
                master.ConsumeItem(849, false);
            }
        }

        // Token: 0x0600075D RID: 1885 RVA: 0x00355BB8 File Offset: 0x00353DB8
        public static bool CheckMech(int i, int j, int time) {
            for (int k = 0; k < _numMechs; k++) {
                if (_mechX[k] == i && _mechY[k] == j) {
                    return false;
                }
            }
            if (_numMechs < 999) {
                _mechX[_numMechs] = i;
                _mechY[_numMechs] = j;
                _mechTime[_numMechs] = time;
                _numMechs++;
                return true;
            }
            return false;
        }

        // Token: 0x0600075E RID: 1886 RVA: 0x00355C2C File Offset: 0x00353E2C
        private static void XferWater() {
            for (int i = 0; i < _numInPump; i++) {
                int num = _inPumpX[i];
                int num2 = _inPumpY[i];
                int liquid = (int)Main.tile[num, num2].liquid;
                if (liquid > 0) {
                    bool flag = Main.tile[num, num2].lava();
                    bool flag2 = Main.tile[num, num2].honey();
                    for (int j = 0; j < _numOutPump; j++) {
                        int num3 = _outPumpX[j];
                        int num4 = _outPumpY[j];
                        int liquid2 = (int)Main.tile[num3, num4].liquid;
                        if (liquid2 < 255) {
                            bool flag3 = Main.tile[num3, num4].lava();
                            bool flag4 = Main.tile[num3, num4].honey();
                            if (liquid2 == 0) {
                                flag3 = flag;
                                flag4 = flag2;
                            }
                            if (flag == flag3 && flag2 == flag4) {
                                int num5 = liquid;
                                if (num5 + liquid2 > 255) {
                                    num5 = 255 - liquid2;
                                }
                                Tile tile = Main.tile[num3, num4];
                                tile.liquid += (byte)num5;
                                Tile tile2 = Main.tile[num, num2];
                                tile2.liquid -= (byte)num5;
                                liquid = (int)Main.tile[num, num2].liquid;
                                Main.tile[num3, num4].lava(flag);
                                Main.tile[num3, num4].honey(flag2);
                                WorldGen.SquareTileFrame(num3, num4, true);
                                if (Main.tile[num, num2].liquid == 0) {
                                    Main.tile[num, num2].lava(false);
                                    WorldGen.SquareTileFrame(num, num2, true);
                                    break;
                                }
                            }
                        }
                    }
                    WorldGen.SquareTileFrame(num, num2, true);
                }
            }
        }

        public static void BigTripWire(int l, int t, int w, int h) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                return;
            }
            TripWire(l, t, w, h);
            PixelBoxPass();
        }

        // Token: 0x0600075F RID: 1887 RVA: 0x00355E08 File Offset: 0x00354008
        private static void TripWire(int left, int top, int width, int height) {
            Wiring.running = true;
            // 清除队列
            if (_wireList.Count != 0) {
                _wireList.Clear(true);
            }
            if (_wireDirectionList.Count != 0) {
                _wireDirectionList.Clear(true);
            }
            Vector2[] array = new Vector2[8];
            int num = 0;
            for (int i = left; i < left + width; i++) {
                for (int j = top; j < top + height; j++) {
                    Point16 back = new Point16(i, j);
                    Tile tile = Main.tile[i, j];
                    if (tile != null && tile.wire()) {
                        _wireList.PushBack(back);
                    }
                }
            }
            _teleport[0].X = -1f;
            _teleport[0].Y = -1f;
            _teleport[1].X = -1f;
            _teleport[1].Y = -1f;
            if (_wireList.Count > 0) {
                _numInPump = 0;
                _numOutPump = 0;
                HitWire(_wireList, 1);
                if (_numInPump > 0 && _numOutPump > 0) {
                    XferWater();
                }
            }
            array[num++] = _teleport[0];
            array[num++] = _teleport[1];
            for (int k = left; k < left + width; k++) {
                for (int l = top; l < top + height; l++) {
                    Point16 back2 = new Point16(k, l);
                    Tile tile2 = Main.tile[k, l];
                    if (tile2 != null && tile2.wire2()) {
                        _wireList.PushBack(back2);
                    }
                }
            }
            _teleport[0].X = -1f;
            _teleport[0].Y = -1f;
            _teleport[1].X = -1f;
            _teleport[1].Y = -1f;
            if (_wireList.Count > 0) {
                _numInPump = 0;
                _numOutPump = 0;
                HitWire(_wireList, 2);
                if (_numInPump > 0 && _numOutPump > 0) {
                    XferWater();
                }
            }
            array[num++] = _teleport[0];
            array[num++] = _teleport[1];
            _teleport[0].X = -1f;
            _teleport[0].Y = -1f;
            _teleport[1].X = -1f;
            _teleport[1].Y = -1f;
            for (int m = left; m < left + width; m++) {
                for (int n = top; n < top + height; n++) {
                    Point16 back3 = new Point16(m, n);
                    Tile tile3 = Main.tile[m, n];
                    if (tile3 != null && tile3.wire3()) {
                        _wireList.PushBack(back3);
                    }
                }
            }
            if (_wireList.Count > 0) {
                _numInPump = 0;
                _numOutPump = 0;
                HitWire(_wireList, 3);
                if (_numInPump > 0 && _numOutPump > 0) {
                    XferWater();
                }
            }
            array[num++] = _teleport[0];
            array[num++] = _teleport[1];
            _teleport[0].X = -1f;
            _teleport[0].Y = -1f;
            _teleport[1].X = -1f;
            _teleport[1].Y = -1f;
            for (int num2 = left; num2 < left + width; num2++) {
                for (int num3 = top; num3 < top + height; num3++) {
                    Point16 back4 = new Point16(num2, num3);
                    Tile tile4 = Main.tile[num2, num3];
                    if (tile4 != null && tile4.wire4()) {
                        _wireList.PushBack(back4);
                    }
                }
            }
            if (_wireList.Count > 0) {
                _numInPump = 0;
                _numOutPump = 0;
                HitWire(_wireList, 4);
                if (_numInPump > 0 && _numOutPump > 0) {
                    XferWater();
                }
            }
            array[num++] = _teleport[0];
            array[num++] = _teleport[1];
            for (int num4 = 0; num4 < 8; num4 += 2) {
                _teleport[0] = array[num4];
                _teleport[1] = array[num4 + 1];
                if (_teleport[0].X >= 0f && _teleport[1].X >= 0f) {
                    Teleport();
                }
            }
            LogicGatePass();
        }

        // Token: 0x06000760 RID: 1888 RVA: 0x00356308 File Offset: 0x00354508
        private static void PixelBoxPass() {
            foreach (KeyValuePair<Point16, byte> current in _wireAccelerator._pixelBoxTriggers) {
                if (current.Value != 2) {
                    if (current.Value == 1) {
                        if (Main.tile[(int)current.Key.X, (int)current.Key.Y].frameX != 0) {
                            Main.tile[(int)current.Key.X, (int)current.Key.Y].frameX = 0;
                            NetMessage.SendTileSquare(-1, (int)current.Key.X, (int)current.Key.Y, 1, TileChangeType.None);
                        }
                    } else if (current.Value == 3 && Main.tile[(int)current.Key.X, (int)current.Key.Y].frameX != 18) {
                        Main.tile[(int)current.Key.X, (int)current.Key.Y].frameX = 18;
                        NetMessage.SendTileSquare(-1, (int)current.Key.X, (int)current.Key.Y, 1, TileChangeType.None);
                    }
                }
            }
            _wireAccelerator._pixelBoxTriggers.Clear();
        }

        // Token: 0x06000761 RID: 1889 RVA: 0x0035647C File Offset: 0x0035467C
        private static void LogicGatePass() {
            if (_GatesCurrent.Count == 0) {
                _GatesDone.Clear();
                while (_LampsToCheck.Count > 0) {
                    while (_LampsToCheck.Count > 0) {
                        Point16 point = _LampsToCheck.Dequeue();
                        CheckLogicGate((int)point.X, (int)point.Y);
                    }
                    while (_GatesNext.Count > 0) {
                        Utils.Swap<Queue<Point16>>(ref _GatesCurrent, ref _GatesNext);
                        while (_GatesCurrent.Count > 0) {
                            Point16 key = _GatesCurrent.Peek();
                            bool flag;
                            if (_GatesDone.TryGetValue(key, out flag) && flag) {
                                _GatesCurrent.Dequeue();
                            } else {
                                _GatesDone.Add(key, true);
                                TripWire((int)key.X, (int)key.Y, 1, 1);
                                _GatesCurrent.Dequeue();
                            }
                        }
                    }
                }
                _GatesDone.Clear();
                if (Wiring.blockPlayerTeleportationForOneIteration) {
                    Wiring.blockPlayerTeleportationForOneIteration = false;
                }
            }
        }

        // Token: 0x06000762 RID: 1890 RVA: 0x00356578 File Offset: 0x00354778
        private static void CheckLogicGate(int lampX, int lampY) {
            if (!WorldGen.InWorld(lampX, lampY, 1)) {
                return;
            }
            int i = lampY;
            while (i < Main.maxTilesY) {
                Tile tile = Main.tile[lampX, i];
                if (!tile.active()) {
                    return;
                }
                // 逻辑门本体而不是输入端
                if (tile.type == 420) {
                    bool flag;
                    _GatesDone.TryGetValue(new Point16(lampX, i), out flag);
                    int num = (int)(tile.frameY / 18);

                    // 逻辑门是否已经处于激活状态
                    bool flag2 = tile.frameX == 18;
                    bool flag3 = tile.frameX == 36;
                    if (num < 0) {
                        return;
                    }
                    int num2 = 0;
                    int num3 = 0;
                    bool flag4 = false;
                    for (int j = i - 1; j > 0; j--) {
                        Tile tile2 = Main.tile[lampX, j];
                        if (!tile2.active() || tile2.type != 419) {
                            break;
                        }
                        //???
                        if (tile2.frameX == 36) {
                            flag4 = true;
                            break;
                        }
                        num2++;
                        num3 += (tile2.frameX == 18).ToInt();
                    }
                    // 逻辑门有没有被激活
                    bool flag5;
                    switch (num) {
                        case 0:
                            flag5 = (num2 == num3);
                            break;
                        case 1:
                            flag5 = (num3 > 0);
                            break;
                        case 2:
                            flag5 = (num2 != num3);
                            break;
                        case 3:
                            flag5 = (num3 == 0);
                            break;
                        case 4:
                            flag5 = (num3 == 1);
                            break;
                        case 5:
                            flag5 = (num3 != 1);
                            break;
                        default:
                            return;
                    }
                    bool flag6 = !flag4 && flag3;
                    bool flag7 = false;
                    if (flag4 && Framing.GetTileSafely(lampX, lampY).frameX == 36) {
                        flag7 = true;
                    }
                    if (flag5 != flag2 || flag6 || flag7) {
                        short num4 = (short)(tile.frameX % 18 / 18);
                        tile.frameX = (short)(18 * flag5.ToInt());
                        if (flag4) {
                            tile.frameX = 36;
                        }
                        SkipWire(lampX, i);
                        WorldGen.SquareTileFrame(lampX, i, true);
                        NetMessage.SendTileSquare(-1, lampX, i, 1, TileChangeType.None);
                        bool flag8 = !flag4 || flag7;
                        if (flag7) {
                            if (num3 != 0) {
                            }
                            flag8 = (Main.rand.NextFloat() < (float)num3 / (float)num2);
                        }
                        if (flag6) {
                            flag8 = false;
                        }
                        if (flag8) {
                            if (!flag) {
                                _GatesNext.Enqueue(new Point16(lampX, i));
                                return;
                            }
                            Vector2 position = new Vector2((float)lampX, (float)i) * 16f - new Vector2(10f);
                            Utils.PoofOfSmoke(position);
                            NetMessage.SendData(MessageID.PoofOfSmoke, -1, -1, null, (int)position.X, position.Y, 0f, 0f, 0, 0, 0);
                        }
                    }
                    return;
                } else {
                    if (tile.type != 419) {
                        return;
                    }
                    i++;
                }
            }
        }

        // Token: 0x06000763 RID: 1891 RVA: 0x003567F4 File Offset: 0x003549F4
        private static void HitWire(DoubleStack<Point16> next, int wireType) {
            _wireDirectionList.Clear(true);
            HashSet<int> visitedConnected = new HashSet<int>();
            for (int i = 0; i < next.Count; i++) {
                Point16 point = next.PopFront();
                WiringWarpper.GetWireAccelerator().Activiate(point.X, point.Y, wireType - 1, visitedConnected);
            }

            _currentWireColor = wireType;
            //while (next.Count > 0) {
            //    Point16 point2 = next.PopFront();
            //    int num = (int)_wireDirectionList.PopFront();
            //    int x = (int)point2.X;
            //    int y = (int)point2.Y;
            //    if (!_wireSkip.ContainsKey(point2)) {
            //        HitWireSingle(x, y);
            //    }
            //    for (int j = 0; j < 4; j++) {
            //        int num2;
            //        int num3;
            //        switch (j) {
            //            case 0:
            //                num2 = x;
            //                num3 = y + 1;
            //                break;
            //            case 1:
            //                num2 = x;
            //                num3 = y - 1;
            //                break;
            //            case 2:
            //                num2 = x + 1;
            //                num3 = y;
            //                break;
            //            case 3:
            //                num2 = x - 1;
            //                num3 = y;
            //                break;
            //            default:
            //                num2 = x;
            //                num3 = y + 1;
            //                break;
            //        }
            //        if (num2 >= 2 && num2 < Main.maxTilesX - 2 && num3 >= 2 && num3 < Main.maxTilesY - 2) {
            //            Tile tile = Main.tile[num2, num3];
            //            if (tile != null) {
            //                Tile tile2 = Main.tile[x, y];
            //                if (tile2 != null) {
            //                    byte b = 3;
            //                    if (tile.type == 424 || tile.type == 445) {
            //                        b = 0;
            //                    }
            //                    if (tile2.type == 424) {
            //                        switch (tile2.frameX / 18) {
            //                            case 0:
            //                                if (j != num) {
            //                                    goto IL_315;
            //                                }
            //                                break;
            //                            case 1:
            //                                if ((num != 0 || j != 3) && (num != 3 || j != 0) && (num != 1 || j != 2)) {
            //                                    if (num != 2) {
            //                                        goto IL_315;
            //                                    }
            //                                    if (j != 1) {
            //                                        goto IL_315;
            //                                    }
            //                                }
            //                                break;
            //                            case 2:
            //                                if ((num != 0 || j != 2) && (num != 2 || j != 0) && (num != 1 || j != 3) && (num != 3 || j != 1)) {
            //                                    goto IL_315;
            //                                }
            //                                break;
            //                        }
            //                    }
            //                    if (tile2.type == 445) {
            //                        if (j != num) {
            //                            goto IL_315;
            //                        }
            //                        if (_PixelBoxTriggers.ContainsKey(point2)) {
            //                            Dictionary<Point16, byte> pixelBoxTriggers;
            //                            Point16 key;
            //                            (pixelBoxTriggers = _PixelBoxTriggers)[key = point2] = ((byte)(pixelBoxTriggers[key] | ((j == 0 | j == 1) ? 2 : 1)));
            //                        } else {
            //                            _PixelBoxTriggers[point2] = (byte)((j == 0 | j == 1) ? 2 : 1);
            //                        }
            //                    }
            //                    bool flag;
            //                    switch (wireType) {
            //                        case 1:
            //                            flag = tile.wire();
            //                            break;
            //                        case 2:
            //                            flag = tile.wire2();
            //                            break;
            //                        case 3:
            //                            flag = tile.wire3();
            //                            break;
            //                        case 4:
            //                            flag = tile.wire4();
            //                            break;
            //                        default:
            //                            flag = false;
            //                            break;
            //                    }
            //                    if (flag) {
            //                        Point16 point3 = new Point16(num2, num3);
            //                        byte b2;
            //                        if (_toProcess.TryGetValue(point3, out b2)) {
            //                            b2 -= 1;
            //                            if (b2 == 0) {
            //                                _toProcess.Remove(point3);
            //                            } else {
            //                                _toProcess[point3] = b2;
            //                            }
            //                        } else {
            //                            next.PushBack(point3);
            //                            _wireDirectionList.PushBack((byte)j);
            //                            if (b > 0) {
            //                                _toProcess.Add(point3, b);
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    IL_315:;
            //    }
            //}
            //_wireSkip.Clear();
            //_toProcess.Clear();
            Wiring.running = false;
        }

        // Token: 0x06000764 RID: 1892 RVA: 0x00356B4C File Offset: 0x00354D4C
        internal static void HitWireSingle(int i, int j) {
            Tile tile = Main.tile[i, j];
            int type = (int)tile.type;
            if (tile.actuator()) {
                ActuateForced(i, j);
            }
            if (tile.active()) {
                if (!TileLoader.PreHitWire(i, j, type)) {
                    return;
                }
                if (type == 144) {
                    HitSwitch(i, j);
                    WorldGen.SquareTileFrame(i, j, true);
                    NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                } else if (type == 421) {
                    if (!tile.actuator()) {
                        tile.type = 422;
                        WorldGen.SquareTileFrame(i, j, true);
                        NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                    }
                } else if (type == 422 && !tile.actuator()) {
                    tile.type = 421;
                    WorldGen.SquareTileFrame(i, j, true);
                    NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                }
                if (type >= 255 && type <= 268) {
                    if (!tile.actuator()) {
                        if (type >= 262) {
                            Tile tile2 = tile;
                            tile2.type -= 7;
                        } else {
                            Tile tile3 = tile;
                            tile3.type += 7;
                        }
                        WorldGen.SquareTileFrame(i, j, true);
                        NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                        return;
                    }
                } else {
                    if (type == 419) {
                        int num = 18;
                        if ((int)tile.frameX >= num) {
                            num = -num;
                        }
                        if (tile.frameX == 36) {
                            num = 0;
                        }
                        SkipWire(i, j);
                        tile.frameX = (short)((int)tile.frameX + num);
                        WorldGen.SquareTileFrame(i, j, true);
                        NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                        _LampsToCheck.Enqueue(new Point16(i, j));
                        return;
                    }
                    if (type == 406) {
                        int num2 = (int)(tile.frameX % 54 / 18);
                        int num3 = (int)(tile.frameY % 54 / 18);
                        int num4 = i - num2;
                        int num5 = j - num3;
                        int num6 = 54;
                        if (Main.tile[num4, num5].frameY >= 108) {
                            num6 = -108;
                        }
                        for (int k = num4; k < num4 + 3; k++) {
                            for (int l = num5; l < num5 + 3; l++) {
                                SkipWire(k, l);
                                Main.tile[k, l].frameY = (short)((int)Main.tile[k, l].frameY + num6);
                            }
                        }
                        NetMessage.SendTileSquare(-1, num4 + 1, num5 + 1, 3, TileChangeType.None);
                        return;
                    }
                    if (type == 452) {
                        int num7 = (int)(tile.frameX % 54 / 18);
                        int num8 = (int)(tile.frameY % 54 / 18);
                        int num9 = i - num7;
                        int num10 = j - num8;
                        int num11 = 54;
                        if (Main.tile[num9, num10].frameX >= 54) {
                            num11 = -54;
                        }
                        for (int m = num9; m < num9 + 3; m++) {
                            for (int n = num10; n < num10 + 3; n++) {
                                SkipWire(m, n);
                                Main.tile[m, n].frameX = (short)((int)Main.tile[m, n].frameX + num11);
                            }
                        }
                        NetMessage.SendTileSquare(-1, num9 + 1, num10 + 1, 3, TileChangeType.None);
                        return;
                    }
                    if (type == 411) {
                        int num12 = (int)(tile.frameX % 36 / 18);
                        int num13 = (int)(tile.frameY % 36 / 18);
                        int num14 = i - num12;
                        int num15 = j - num13;
                        int num16 = 36;
                        if (Main.tile[num14, num15].frameX >= 36) {
                            num16 = -36;
                        }
                        for (int num17 = num14; num17 < num14 + 2; num17++) {
                            for (int num18 = num15; num18 < num15 + 2; num18++) {
                                SkipWire(num17, num18);
                                Main.tile[num17, num18].frameX = (short)((int)Main.tile[num17, num18].frameX + num16);
                            }
                        }
                        NetMessage.SendTileSquare(-1, num14, num15, 2, TileChangeType.None);
                        return;
                    }
                    if (type == 425) {
                        int num19 = (int)(tile.frameX % 36 / 18);
                        int num20 = (int)(tile.frameY % 36 / 18);
                        int num21 = i - num19;
                        int num22 = j - num20;
                        for (int num23 = num21; num23 < num21 + 2; num23++) {
                            for (int num24 = num22; num24 < num22 + 2; num24++) {
                                SkipWire(num23, num24);
                            }
                        }
                        if (!Main.AnnouncementBoxDisabled) {
                            Color pink = Color.Pink;
                            int num25 = Sign.ReadSign(num21, num22, false);
                            if (num25 != -1 && Main.sign[num25] != null && !string.IsNullOrWhiteSpace(Main.sign[num25].text)) {
                                if (Main.AnnouncementBoxRange == -1) {
                                    if (Main.netMode == NetmodeID.SinglePlayer) {
                                        Main.NewTextMultiline(Main.sign[num25].text, false, pink, 460);
                                        return;
                                    }
                                    if (Main.netMode == NetmodeID.Server) {
                                        NetMessage.SendData(MessageID.SmartTextMessage, -1, -1, NetworkText.FromLiteral(Main.sign[num25].text), 255, (float)pink.R, (float)pink.G, (float)pink.B, 460, 0, 0);
                                        return;
                                    }
                                } else if (Main.netMode == NetmodeID.SinglePlayer) {
                                    if (Main.player[Main.myPlayer].Distance(new Vector2((float)(num21 * 16 + 16), (float)(num22 * 16 + 16))) <= (float)Main.AnnouncementBoxRange) {
                                        Main.NewTextMultiline(Main.sign[num25].text, false, pink, 460);
                                        return;
                                    }
                                } else if (Main.netMode == NetmodeID.Server) {
                                    for (int num26 = 0; num26 < 255; num26++) {
                                        if (Main.player[num26].active && Main.player[num26].Distance(new Vector2((float)(num21 * 16 + 16), (float)(num22 * 16 + 16))) <= (float)Main.AnnouncementBoxRange) {
                                            NetMessage.SendData(MessageID.SmartTextMessage, num26, -1, NetworkText.FromLiteral(Main.sign[num25].text), 255, (float)pink.R, (float)pink.G, (float)pink.B, 460, 0, 0);
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                    } else {
                        if (type == 405) {
                            int num27 = (int)(tile.frameX % 54 / 18);
                            int num28 = (int)(tile.frameY % 36 / 18);
                            int num29 = i - num27;
                            int num30 = j - num28;
                            int num31 = 54;
                            if (Main.tile[num29, num30].frameX >= 54) {
                                num31 = -54;
                            }
                            for (int num32 = num29; num32 < num29 + 3; num32++) {
                                for (int num33 = num30; num33 < num30 + 2; num33++) {
                                    SkipWire(num32, num33);
                                    Main.tile[num32, num33].frameX = (short)((int)Main.tile[num32, num33].frameX + num31);
                                }
                            }
                            NetMessage.SendTileSquare(-1, num29 + 1, num30 + 1, 3, TileChangeType.None);
                            return;
                        }
                        if (type == 209) {
                            int num34 = (int)(tile.frameX % 72 / 18);
                            int num35 = (int)(tile.frameY % 54 / 18);
                            int num36 = i - num34;
                            int num37 = j - num35;
                            int num38 = (int)(tile.frameY / 54);
                            int num39 = (int)(tile.frameX / 72);
                            int num40 = -1;
                            if (num34 == 1 || num34 == 2) {
                                num40 = num35;
                            }
                            int num41 = 0;
                            if (num34 == 3) {
                                num41 = -54;
                            }
                            if (num34 == 0) {
                                num41 = 54;
                            }
                            if (num38 >= 8 && num41 > 0) {
                                num41 = 0;
                            }
                            if (num38 == 0 && num41 < 0) {
                                num41 = 0;
                            }
                            bool flag = false;
                            if (num41 != 0) {
                                for (int num42 = num36; num42 < num36 + 4; num42++) {
                                    for (int num43 = num37; num43 < num37 + 3; num43++) {
                                        SkipWire(num42, num43);
                                        Main.tile[num42, num43].frameY = (short)((int)Main.tile[num42, num43].frameY + num41);
                                    }
                                }
                                flag = true;
                            }
                            if ((num39 == 3 || num39 == 4) && (num40 == 0 || num40 == 1)) {
                                num41 = ((num39 == 3) ? 72 : -72);
                                for (int num44 = num36; num44 < num36 + 4; num44++) {
                                    for (int num45 = num37; num45 < num37 + 3; num45++) {
                                        SkipWire(num44, num45);
                                        Main.tile[num44, num45].frameX = (short)((int)Main.tile[num44, num45].frameX + num41);
                                    }
                                }
                                flag = true;
                            }
                            if (flag) {
                                NetMessage.SendTileSquare(-1, num36 + 1, num37 + 1, 4, TileChangeType.None);
                            }
                            if (num40 != -1) {
                                bool flag2 = true;
                                if ((num39 == 3 || num39 == 4) && num40 < 2) {
                                    flag2 = false;
                                }
                                if (CheckMech(num36, num37, 30) && flag2) {
                                    WorldGen.ShootFromCannon(num36, num37, num38, num39 + 1, 0, 0f, CurrentUser);
                                    return;
                                }
                            }
                        } else if (type == 212) {
                            int num46 = (int)(tile.frameX % 54 / 18);
                            int num47 = (int)(tile.frameY % 54 / 18);
                            int num48 = i - num46;
                            int num49 = j - num47;
                            short num148 = (short)(tile.frameX / 54);
                            int num50 = -1;
                            if (num46 == 1) {
                                num50 = num47;
                            }
                            int num51 = 0;
                            if (num46 == 0) {
                                num51 = -54;
                            }
                            if (num46 == 2) {
                                num51 = 54;
                            }
                            if (num148 >= 1 && num51 > 0) {
                                num51 = 0;
                            }
                            if (num148 == 0 && num51 < 0) {
                                num51 = 0;
                            }
                            bool flag3 = false;
                            if (num51 != 0) {
                                for (int num52 = num48; num52 < num48 + 3; num52++) {
                                    for (int num53 = num49; num53 < num49 + 3; num53++) {
                                        SkipWire(num52, num53);
                                        Main.tile[num52, num53].frameX = (short)((int)Main.tile[num52, num53].frameX + num51);
                                    }
                                }
                                flag3 = true;
                            }
                            if (flag3) {
                                NetMessage.SendTileSquare(-1, num48 + 1, num49 + 1, 4, TileChangeType.None);
                            }
                            if (num50 != -1 && CheckMech(num48, num49, 10)) {
                                float num149 = 12f + (float)Main.rand.Next(450) * 0.01f;
                                float num54 = (float)Main.rand.Next(85, 105);
                                float num150 = (float)Main.rand.Next(-35, 11);
                                int type2 = 166;
                                int damage = 0;
                                float knockBack = 0f;
                                Vector2 vector = new Vector2((float)((num48 + 2) * 16 - 8), (float)((num49 + 2) * 16 - 8));
                                if (tile.frameX / 54 == 0) {
                                    num54 *= -1f;
                                    vector.X -= 12f;
                                } else {
                                    vector.X += 12f;
                                }
                                float num55 = num54;
                                float num56 = num150;
                                float num57 = (float)Math.Sqrt((double)(num55 * num55 + num56 * num56));
                                num57 = num149 / num57;
                                num55 *= num57;
                                num56 *= num57;
                                Projectile.NewProjectile(vector.X, vector.Y, num55, num56, type2, damage, knockBack, CurrentUser, 0f, 0f);
                                return;
                            }
                        } else {
                            if (type == 215) {
                                int num58 = (int)(tile.frameX % 54 / 18);
                                int num59 = (int)(tile.frameY % 36 / 18);
                                int num60 = i - num58;
                                int num61 = j - num59;
                                int num62 = 36;
                                if (Main.tile[num60, num61].frameY >= 36) {
                                    num62 = -36;
                                }
                                for (int num63 = num60; num63 < num60 + 3; num63++) {
                                    for (int num64 = num61; num64 < num61 + 2; num64++) {
                                        SkipWire(num63, num64);
                                        Main.tile[num63, num64].frameY = (short)((int)Main.tile[num63, num64].frameY + num62);
                                    }
                                }
                                NetMessage.SendTileSquare(-1, num60 + 1, num61 + 1, 3, TileChangeType.None);
                                return;
                            }
                            if (type == 130) {
                                if (Main.tile[i, j - 1] != null && Main.tile[i, j - 1].active()) {
                                    if (TileID.Sets.BasicChest[(int)Main.tile[i, j - 1].type] || TileID.Sets.BasicChestFake[(int)Main.tile[i, j - 1].type]) {
                                        return;
                                    }
                                    if (Main.tile[i, j - 1].type == 88) {
                                        return;
                                    }
                                }
                                tile.type = 131;
                                WorldGen.SquareTileFrame(i, j, true);
                                NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                                return;
                            }
                            if (type == 131) {
                                tile.type = 130;
                                WorldGen.SquareTileFrame(i, j, true);
                                NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                                return;
                            }
                            if (type == 387 || type == 386) {
                                bool value = type == 387;
                                int num65 = WorldGen.ShiftTrapdoor(i, j, true, -1).ToInt();
                                if (num65 == 0) {
                                    num65 = -WorldGen.ShiftTrapdoor(i, j, false, -1).ToInt();
                                }
                                if (num65 != 0) {
                                    NetMessage.SendData(MessageID.ChangeDoor, -1, -1, null, 3 - value.ToInt(), (float)i, (float)j, (float)num65, 0, 0, 0);
                                    return;
                                }
                            } else {
                                if (type == 389 || type == 388) {
                                    bool flag4 = type == 389;
                                    WorldGen.ShiftTallGate(i, j, flag4);
                                    NetMessage.SendData(MessageID.ChangeDoor, -1, -1, null, 4 + flag4.ToInt(), (float)i, (float)j, 0f, 0, 0, 0);
                                    return;
                                }
                                if (TileLoader.CloseDoorID(Main.tile[i, j]) >= 0) {
                                    if (WorldGen.CloseDoor(i, j, true)) {
                                        NetMessage.SendData(MessageID.ChangeDoor, -1, -1, null, 1, (float)i, (float)j, 0f, 0, 0, 0);
                                        return;
                                    }
                                } else if (TileLoader.OpenDoorID(Main.tile[i, j]) >= 0) {
                                    int num66 = 1;
                                    if (Main.rand.Next(2) == 0) {
                                        num66 = -1;
                                    }
                                    if (WorldGen.OpenDoor(i, j, num66)) {
                                        NetMessage.SendData(MessageID.ChangeDoor, -1, -1, null, 0, (float)i, (float)j, (float)num66, 0, 0, 0);
                                        return;
                                    }
                                    if (WorldGen.OpenDoor(i, j, -num66)) {
                                        NetMessage.SendData(MessageID.ChangeDoor, -1, -1, null, 0, (float)i, (float)j, -(float)num66, 0, 0, 0);
                                        return;
                                    }
                                } else {
                                    if (type == 216) {
                                        WorldGen.LaunchRocket(i, j);
                                        SkipWire(i, j);
                                        return;
                                    }
                                    if (type == 335) {
                                        int num67 = j - (int)(tile.frameY / 18);
                                        int num68 = i - (int)(tile.frameX / 18);
                                        SkipWire(num68, num67);
                                        SkipWire(num68, num67 + 1);
                                        SkipWire(num68 + 1, num67);
                                        SkipWire(num68 + 1, num67 + 1);
                                        if (CheckMech(num68, num67, 30)) {
                                            WorldGen.LaunchRocketSmall(num68, num67);
                                            return;
                                        }
                                    } else if (type == 338) {
                                        int num69 = j - (int)(tile.frameY / 18);
                                        int num70 = i - (int)(tile.frameX / 18);
                                        SkipWire(num70, num69);
                                        SkipWire(num70, num69 + 1);
                                        if (CheckMech(num70, num69, 30)) {
                                            bool flag5 = false;
                                            for (int num71 = 0; num71 < 1000; num71++) {
                                                if (Main.projectile[num71].active && Main.projectile[num71].aiStyle == 73 && Main.projectile[num71].ai[0] == (float)num70 && Main.projectile[num71].ai[1] == (float)num69) {
                                                    flag5 = true;
                                                    break;
                                                }
                                            }
                                            if (!flag5) {
                                                Projectile.NewProjectile((float)(num70 * 16 + 8), (float)(num69 * 16 + 2), 0f, 0f, 419 + Main.rand.Next(4), 0, 0f, Main.myPlayer, (float)num70, (float)num69);
                                                return;
                                            }
                                        }
                                    } else if (type == 235) {
                                        int num72 = i - (int)(tile.frameX / 18);
                                        if (tile.wall == 87 && (double)j > Main.worldSurface && !NPC.downedPlantBoss) {
                                            return;
                                        }
                                        if (_teleport[0].X == -1f) {
                                            _teleport[0].X = (float)num72;
                                            _teleport[0].Y = (float)j;
                                            if (tile.halfBrick()) {
                                                Vector2[] expr_EFC_cp_0 = _teleport;
                                                int expr_EFC_cp_ = 0;
                                                expr_EFC_cp_0[expr_EFC_cp_].Y = expr_EFC_cp_0[expr_EFC_cp_].Y + 0.5f;
                                                return;
                                            }
                                        } else if (_teleport[0].X != (float)num72 || _teleport[0].Y != (float)j) {
                                            _teleport[1].X = (float)num72;
                                            _teleport[1].Y = (float)j;
                                            if (tile.halfBrick()) {
                                                Vector2[] expr_F75_cp_0 = _teleport;
                                                int expr_F75_cp_ = 1;
                                                expr_F75_cp_0[expr_F75_cp_].Y = expr_F75_cp_0[expr_F75_cp_].Y + 0.5f;
                                                return;
                                            }
                                        }
                                    } else {
                                        if (TileLoader.IsTorch(type)) {
                                            if (tile.frameX < 66) {
                                                Tile tile4 = tile;
                                                tile4.frameX += 66;
                                            } else {
                                                Tile tile5 = tile;
                                                tile5.frameX -= 66;
                                            }
                                            NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                                            return;
                                        }
                                        if (type == 429) {
                                            short num151 = (short)(Main.tile[i, j].frameX / 18);
                                            bool flag6 = num151 % 2 >= 1;
                                            bool flag7 = num151 % 4 >= 2;
                                            bool flag8 = num151 % 8 >= 4;
                                            bool flag9 = num151 % 16 >= 8;
                                            bool flag10 = false;
                                            short num73 = 0;
                                            switch (_currentWireColor) {
                                                case 1:
                                                    num73 = 18;
                                                    flag10 = !flag6;
                                                    break;
                                                case 2:
                                                    num73 = 72;
                                                    flag10 = !flag8;
                                                    break;
                                                case 3:
                                                    num73 = 36;
                                                    flag10 = !flag7;
                                                    break;
                                                case 4:
                                                    num73 = 144;
                                                    flag10 = !flag9;
                                                    break;
                                            }
                                            if (flag10) {
                                                Tile tile6 = tile;
                                                tile6.frameX += num73;
                                            } else {
                                                Tile tile7 = tile;
                                                tile7.frameX -= num73;
                                            }
                                            NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                                            return;
                                        }
                                        if (type == 149) {
                                            if (tile.frameX < 54) {
                                                Tile tile8 = tile;
                                                tile8.frameX += 54;
                                            } else {
                                                Tile tile9 = tile;
                                                tile9.frameX -= 54;
                                            }
                                            NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                                            return;
                                        }
                                        if (type == 244) {
                                            int num74;
                                            for (num74 = (int)(tile.frameX / 18); num74 >= 3; num74 -= 3) {
                                            }
                                            int num75;
                                            for (num75 = (int)(tile.frameY / 18); num75 >= 3; num75 -= 3) {
                                            }
                                            int num76 = i - num74;
                                            int num77 = j - num75;
                                            int num78 = 54;
                                            if (Main.tile[num76, num77].frameX >= 54) {
                                                num78 = -54;
                                            }
                                            for (int num79 = num76; num79 < num76 + 3; num79++) {
                                                for (int num80 = num77; num80 < num77 + 2; num80++) {
                                                    SkipWire(num79, num80);
                                                    Main.tile[num79, num80].frameX = (short)((int)Main.tile[num79, num80].frameX + num78);
                                                }
                                            }
                                            NetMessage.SendTileSquare(-1, num76 + 1, num77 + 1, 3, TileChangeType.None);
                                            return;
                                        }
                                        if (type == 42) {
                                            int num81;
                                            for (num81 = (int)(tile.frameY / 18); num81 >= 2; num81 -= 2) {
                                            }
                                            int num82 = j - num81;
                                            short num83 = 18;
                                            if (tile.frameX > 0) {
                                                num83 = -18;
                                            }
                                            Tile tile10 = Main.tile[i, num82];
                                            tile10.frameX += num83;
                                            Tile tile11 = Main.tile[i, num82 + 1];
                                            tile11.frameX += num83;
                                            SkipWire(i, num82);
                                            SkipWire(i, num82 + 1);
                                            NetMessage.SendTileSquare(-1, i, j, 2, TileChangeType.None);
                                            return;
                                        }
                                        if (type == 93) {
                                            int num84;
                                            for (num84 = (int)(tile.frameY / 18); num84 >= 3; num84 -= 3) {
                                            }
                                            num84 = j - num84;
                                            short num85 = 18;
                                            if (tile.frameX > 0) {
                                                num85 = -18;
                                            }
                                            Tile tile12 = Main.tile[i, num84];
                                            tile12.frameX += num85;
                                            Tile tile13 = Main.tile[i, num84 + 1];
                                            tile13.frameX += num85;
                                            Tile tile14 = Main.tile[i, num84 + 2];
                                            tile14.frameX += num85;
                                            SkipWire(i, num84);
                                            SkipWire(i, num84 + 1);
                                            SkipWire(i, num84 + 2);
                                            NetMessage.SendTileSquare(-1, i, num84 + 1, 3, TileChangeType.None);
                                            return;
                                        }
                                        if (type == 126 || type == 95 || type == 100 || type == 173) {
                                            int num86;
                                            for (num86 = (int)(tile.frameY / 18); num86 >= 2; num86 -= 2) {
                                            }
                                            num86 = j - num86;
                                            int num87 = (int)(tile.frameX / 18);
                                            if (num87 > 1) {
                                                num87 -= 2;
                                            }
                                            num87 = i - num87;
                                            short num88 = 36;
                                            if (Main.tile[num87, num86].frameX > 0) {
                                                num88 = -36;
                                            }
                                            Tile tile15 = Main.tile[num87, num86];
                                            tile15.frameX += num88;
                                            Tile tile16 = Main.tile[num87, num86 + 1];
                                            tile16.frameX += num88;
                                            Tile tile17 = Main.tile[num87 + 1, num86];
                                            tile17.frameX += num88;
                                            Tile tile18 = Main.tile[num87 + 1, num86 + 1];
                                            tile18.frameX += num88;
                                            SkipWire(num87, num86);
                                            SkipWire(num87 + 1, num86);
                                            SkipWire(num87, num86 + 1);
                                            SkipWire(num87 + 1, num86 + 1);
                                            NetMessage.SendTileSquare(-1, num87, num86, 3, TileChangeType.None);
                                            return;
                                        }
                                        if (type == 34) {
                                            int num89;
                                            for (num89 = (int)(tile.frameY / 18); num89 >= 3; num89 -= 3) {
                                            }
                                            int num90 = j - num89;
                                            int num91 = (int)(tile.frameX % 108 / 18);
                                            if (num91 > 2) {
                                                num91 -= 3;
                                            }
                                            num91 = i - num91;
                                            short num92 = 54;
                                            if (Main.tile[num91, num90].frameX % 108 > 0) {
                                                num92 = -54;
                                            }
                                            for (int num93 = num91; num93 < num91 + 3; num93++) {
                                                for (int num94 = num90; num94 < num90 + 3; num94++) {
                                                    Tile tile19 = Main.tile[num93, num94];
                                                    tile19.frameX += num92;
                                                    SkipWire(num93, num94);
                                                }
                                            }
                                            NetMessage.SendTileSquare(-1, num91 + 1, num90 + 1, 3, TileChangeType.None);
                                            return;
                                        }
                                        if (type == 314) {
                                            if (CheckMech(i, j, 5)) {
                                                Minecart.FlipSwitchTrack(i, j);
                                                return;
                                            }
                                        } else {
                                            if (type == 33 || type == 174) {
                                                short num95 = 18;
                                                if (tile.frameX > 0) {
                                                    num95 = -18;
                                                }
                                                Tile tile20 = tile;
                                                tile20.frameX += num95;
                                                NetMessage.SendTileSquare(-1, i, j, 3, TileChangeType.None);
                                                return;
                                            }
                                            if (type == 92) {
                                                int num96 = j - (int)(tile.frameY / 18);
                                                short num97 = 18;
                                                if (tile.frameX > 0) {
                                                    num97 = -18;
                                                }
                                                for (int num98 = num96; num98 < num96 + 6; num98++) {
                                                    Tile tile21 = Main.tile[i, num98];
                                                    tile21.frameX += num97;
                                                    SkipWire(i, num98);
                                                }
                                                NetMessage.SendTileSquare(-1, i, num96 + 3, 7, TileChangeType.None);
                                                return;
                                            }
                                            if (type == 137) {
                                                int num99 = (int)(tile.frameY / 18);
                                                Vector2 zero = Vector2.Zero;
                                                float speedX = 0f;
                                                float speedY = 0f;
                                                int num100 = 0;
                                                int damage2 = 0;
                                                switch (num99) {
                                                    case 0:
                                                    case 1:
                                                    case 2:
                                                        if (CheckMech(i, j, 200)) {
                                                            int num101 = (tile.frameX == 0) ? -1 : ((tile.frameX == 18) ? 1 : 0);
                                                            int num102 = (tile.frameX < 36) ? 0 : ((tile.frameX < 72) ? -1 : 1);
                                                            zero = new Vector2((float)(i * 16 + 8 + 10 * num101), (float)(j * 16 + 9 + num102 * 9));
                                                            float num103 = 3f;
                                                            if (num99 == 0) {
                                                                num100 = 98;
                                                                damage2 = 20;
                                                                num103 = 12f;
                                                            }
                                                            if (num99 == 1) {
                                                                num100 = 184;
                                                                damage2 = 40;
                                                                num103 = 12f;
                                                            }
                                                            if (num99 == 2) {
                                                                num100 = 187;
                                                                damage2 = 40;
                                                                num103 = 5f;
                                                            }
                                                            speedX = (float)num101 * num103;
                                                            speedY = (float)num102 * num103;
                                                        }
                                                        break;
                                                    case 3:
                                                        if (CheckMech(i, j, 300)) {
                                                            int num104 = 200;
                                                            for (int num105 = 0; num105 < 1000; num105++) {
                                                                if (Main.projectile[num105].active && Main.projectile[num105].type == num100) {
                                                                    float num106 = (new Vector2((float)(i * 16 + 8), (float)(j * 18 + 8)) - Main.projectile[num105].Center).Length();
                                                                    if (num106 < 50f) {
                                                                        num104 -= 50;
                                                                    } else if (num106 < 100f) {
                                                                        num104 -= 15;
                                                                    } else if (num106 < 200f) {
                                                                        num104 -= 10;
                                                                    } else if (num106 < 300f) {
                                                                        num104 -= 8;
                                                                    } else if (num106 < 400f) {
                                                                        num104 -= 6;
                                                                    } else if (num106 < 500f) {
                                                                        num104 -= 5;
                                                                    } else if (num106 < 700f) {
                                                                        num104 -= 4;
                                                                    } else if (num106 < 900f) {
                                                                        num104 -= 3;
                                                                    } else if (num106 < 1200f) {
                                                                        num104 -= 2;
                                                                    } else {
                                                                        num104--;
                                                                    }
                                                                }
                                                            }
                                                            if (num104 > 0) {
                                                                num100 = 185;
                                                                damage2 = 40;
                                                                int num107 = 0;
                                                                int num108 = 0;
                                                                switch (tile.frameX / 18) {
                                                                    case 0:
                                                                    case 1:
                                                                        num107 = 0;
                                                                        num108 = 1;
                                                                        break;
                                                                    case 2:
                                                                        num107 = 0;
                                                                        num108 = -1;
                                                                        break;
                                                                    case 3:
                                                                        num107 = -1;
                                                                        num108 = 0;
                                                                        break;
                                                                    case 4:
                                                                        num107 = 1;
                                                                        num108 = 0;
                                                                        break;
                                                                }
                                                                speedX = (float)(4 * num107) + (float)Main.rand.Next(-20 + ((num107 == 1) ? 20 : 0), 21 - ((num107 == -1) ? 20 : 0)) * 0.05f;
                                                                speedY = (float)(4 * num108) + (float)Main.rand.Next(-20 + ((num108 == 1) ? 20 : 0), 21 - ((num108 == -1) ? 20 : 0)) * 0.05f;
                                                                zero = new Vector2((float)(i * 16 + 8 + 14 * num107), (float)(j * 16 + 8 + 14 * num108));
                                                            }
                                                        }
                                                        break;
                                                    case 4:
                                                        if (CheckMech(i, j, 90)) {
                                                            int num109 = 0;
                                                            int num110 = 0;
                                                            switch (tile.frameX / 18) {
                                                                case 0:
                                                                case 1:
                                                                    num109 = 0;
                                                                    num110 = 1;
                                                                    break;
                                                                case 2:
                                                                    num109 = 0;
                                                                    num110 = -1;
                                                                    break;
                                                                case 3:
                                                                    num109 = -1;
                                                                    num110 = 0;
                                                                    break;
                                                                case 4:
                                                                    num109 = 1;
                                                                    num110 = 0;
                                                                    break;
                                                            }
                                                            speedX = (float)(8 * num109);
                                                            speedY = (float)(8 * num110);
                                                            damage2 = 60;
                                                            num100 = 186;
                                                            zero = new Vector2((float)(i * 16 + 8 + 18 * num109), (float)(j * 16 + 8 + 18 * num110));
                                                        }
                                                        break;
                                                }
                                                switch (num99 + 10) {
                                                    case 0:
                                                        if (CheckMech(i, j, 200)) {
                                                            int num111 = -1;
                                                            if (tile.frameX != 0) {
                                                                num111 = 1;
                                                            }
                                                            speedX = (float)(12 * num111);
                                                            damage2 = 20;
                                                            num100 = 98;
                                                            zero = new Vector2((float)(i * 16 + 8), (float)(j * 16 + 7));
                                                            zero.X += (float)(10 * num111);
                                                            zero.Y += 2f;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (CheckMech(i, j, 200)) {
                                                            int num112 = -1;
                                                            if (tile.frameX != 0) {
                                                                num112 = 1;
                                                            }
                                                            speedX = (float)(12 * num112);
                                                            damage2 = 40;
                                                            num100 = 184;
                                                            zero = new Vector2((float)(i * 16 + 8), (float)(j * 16 + 7));
                                                            zero.X += (float)(10 * num112);
                                                            zero.Y += 2f;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (CheckMech(i, j, 200)) {
                                                            int num113 = -1;
                                                            if (tile.frameX != 0) {
                                                                num113 = 1;
                                                            }
                                                            speedX = (float)(5 * num113);
                                                            damage2 = 40;
                                                            num100 = 187;
                                                            zero = new Vector2((float)(i * 16 + 8), (float)(j * 16 + 7));
                                                            zero.X += (float)(10 * num113);
                                                            zero.Y += 2f;
                                                        }
                                                        break;
                                                    case 3:
                                                        if (CheckMech(i, j, 300)) {
                                                            num100 = 185;
                                                            int num114 = 200;
                                                            for (int num115 = 0; num115 < 1000; num115++) {
                                                                if (Main.projectile[num115].active && Main.projectile[num115].type == num100) {
                                                                    float num116 = (new Vector2((float)(i * 16 + 8), (float)(j * 18 + 8)) - Main.projectile[num115].Center).Length();
                                                                    if (num116 < 50f) {
                                                                        num114 -= 50;
                                                                    } else if (num116 < 100f) {
                                                                        num114 -= 15;
                                                                    } else if (num116 < 200f) {
                                                                        num114 -= 10;
                                                                    } else if (num116 < 300f) {
                                                                        num114 -= 8;
                                                                    } else if (num116 < 400f) {
                                                                        num114 -= 6;
                                                                    } else if (num116 < 500f) {
                                                                        num114 -= 5;
                                                                    } else if (num116 < 700f) {
                                                                        num114 -= 4;
                                                                    } else if (num116 < 900f) {
                                                                        num114 -= 3;
                                                                    } else if (num116 < 1200f) {
                                                                        num114 -= 2;
                                                                    } else {
                                                                        num114--;
                                                                    }
                                                                }
                                                            }
                                                            if (num114 > 0) {
                                                                speedX = (float)Main.rand.Next(-20, 21) * 0.05f;
                                                                speedY = 4f + (float)Main.rand.Next(0, 21) * 0.05f;
                                                                damage2 = 40;
                                                                zero = new Vector2((float)(i * 16 + 8), (float)(j * 16 + 16));
                                                                zero.Y += 6f;
                                                                Projectile.NewProjectile((float)((int)zero.X), (float)((int)zero.Y), speedX, speedY, num100, damage2, 2f, Main.myPlayer, 0f, 0f);
                                                            }
                                                        }
                                                        break;
                                                    case 4:
                                                        if (CheckMech(i, j, 90)) {
                                                            speedX = 0f;
                                                            speedY = 8f;
                                                            damage2 = 60;
                                                            num100 = 186;
                                                            zero = new Vector2((float)(i * 16 + 8), (float)(j * 16 + 16));
                                                            zero.Y += 10f;
                                                        }
                                                        break;
                                                }
                                                if (num100 != 0) {
                                                    Projectile.NewProjectile((float)((int)zero.X), (float)((int)zero.Y), speedX, speedY, num100, damage2, 2f, Main.myPlayer, 0f, 0f);
                                                    return;
                                                }
                                            } else if (type == 443) {
                                                int num117 = (int)(tile.frameX / 36);
                                                int num118 = i - ((int)tile.frameX - num117 * 36) / 18;
                                                if (CheckMech(num118, j, 200)) {
                                                    Vector2 vector2 = Vector2.Zero;
                                                    Vector2 zero2 = Vector2.Zero;
                                                    int num119 = 654;
                                                    int damage3 = 20;
                                                    if (num117 < 2) {
                                                        vector2 = new Vector2((float)(num118 + 1), (float)j) * 16f;
                                                        zero2 = new Vector2(0f, -8f);
                                                    } else {
                                                        vector2 = new Vector2((float)(num118 + 1), (float)(j + 1)) * 16f;
                                                        zero2 = new Vector2(0f, 8f);
                                                    }
                                                    if (num119 != 0) {
                                                        Projectile.NewProjectile((float)((int)vector2.X), (float)((int)vector2.Y), zero2.X, zero2.Y, num119, damage3, 2f, Main.myPlayer, 0f, 0f);
                                                        return;
                                                    }
                                                }
                                            } else {
                                                if (type == 139 || type == 35 || TileLoader.IsModMusicBox(tile)) {
                                                    WorldGen.SwitchMB(i, j);
                                                    return;
                                                }
                                                if (type == 207) {
                                                    WorldGen.SwitchFountain(i, j);
                                                    return;
                                                }
                                                if (type == 410) {
                                                    WorldGen.SwitchMonolith(i, j);
                                                    return;
                                                }
                                                if (type == 455) {
                                                    BirthdayParty.ToggleManualParty();
                                                    return;
                                                }
                                                if (type == 141) {
                                                    WorldGen.KillTile(i, j, false, false, true);
                                                    NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                                                    Projectile.NewProjectile((float)(i * 16 + 8), (float)(j * 16 + 8), 0f, 0f, ProjectileID.Explosives, 500, 10f, Main.myPlayer, 0f, 0f);
                                                    return;
                                                }
                                                if (type == 210) {
                                                    WorldGen.ExplodeMine(i, j);
                                                    return;
                                                }
                                                if (type == 142 || type == 143) {
                                                    int num120 = j - (int)(tile.frameY / 18);
                                                    int num121 = (int)(tile.frameX / 18);
                                                    if (num121 > 1) {
                                                        num121 -= 2;
                                                    }
                                                    num121 = i - num121;
                                                    SkipWire(num121, num120);
                                                    SkipWire(num121, num120 + 1);
                                                    SkipWire(num121 + 1, num120);
                                                    SkipWire(num121 + 1, num120 + 1);
                                                    if (type == 142) {
                                                        for (int num122 = 0; num122 < 4; num122++) {
                                                            if (_numInPump >= 19) {
                                                                return;
                                                            }
                                                            int num123;
                                                            int num124;
                                                            if (num122 == 0) {
                                                                num123 = num121;
                                                                num124 = num120 + 1;
                                                            } else if (num122 == 1) {
                                                                num123 = num121 + 1;
                                                                num124 = num120 + 1;
                                                            } else if (num122 == 2) {
                                                                num123 = num121;
                                                                num124 = num120;
                                                            } else {
                                                                num123 = num121 + 1;
                                                                num124 = num120;
                                                            }
                                                            _inPumpX[_numInPump] = num123;
                                                            _inPumpY[_numInPump] = num124;
                                                            _numInPump++;
                                                        }
                                                        return;
                                                    }
                                                    for (int num125 = 0; num125 < 4; num125++) {
                                                        if (_numOutPump >= 19) {
                                                            return;
                                                        }
                                                        int num126;
                                                        int num127;
                                                        if (num125 == 0) {
                                                            num126 = num121;
                                                            num127 = num120 + 1;
                                                        } else if (num125 == 1) {
                                                            num126 = num121 + 1;
                                                            num127 = num120 + 1;
                                                        } else if (num125 == 2) {
                                                            num126 = num121;
                                                            num127 = num120;
                                                        } else {
                                                            num126 = num121 + 1;
                                                            num127 = num120;
                                                        }
                                                        _outPumpX[_numOutPump] = num126;
                                                        _outPumpY[_numOutPump] = num127;
                                                        _numOutPump++;
                                                    }
                                                    return;
                                                } else if (type == 105) {
                                                    int num128 = j - (int)(tile.frameY / 18);
                                                    int num129 = (int)(tile.frameX / 18);
                                                    int num130 = 0;
                                                    while (num129 >= 2) {
                                                        num129 -= 2;
                                                        num130++;
                                                    }
                                                    num129 = i - num129;
                                                    num129 = i - (int)(tile.frameX % 36 / 18);
                                                    num128 = j - (int)(tile.frameY % 54 / 18);
                                                    num130 = (int)(tile.frameX / 36 + tile.frameY / 54 * 55);
                                                    SkipWire(num129, num128);
                                                    SkipWire(num129, num128 + 1);
                                                    SkipWire(num129, num128 + 2);
                                                    SkipWire(num129 + 1, num128);
                                                    SkipWire(num129 + 1, num128 + 1);
                                                    SkipWire(num129 + 1, num128 + 2);
                                                    int num131 = num129 * 16 + 16;
                                                    int num132 = (num128 + 3) * 16;
                                                    int num133 = -1;
                                                    int num134 = -1;
                                                    bool flag11 = true;
                                                    bool flag12 = false;
                                                    switch (num130) {
                                                        case 51:
                                                            num134 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                                            {
                                                            299,
                                                            538
                                                            });
                                                            break;
                                                        case 52:
                                                            num134 = 356;
                                                            break;
                                                        case 53:
                                                            num134 = 357;
                                                            break;
                                                        case 54:
                                                            num134 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                                            {
                                                            355,
                                                            358
                                                            });
                                                            break;
                                                        case 55:
                                                            num134 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                                            {
                                                            367,
                                                            366
                                                            });
                                                            break;
                                                        case 56:
                                                            num134 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                                            {
                                                            359,
                                                            359,
                                                            359,
                                                            359,
                                                            360
                                                            });
                                                            break;
                                                        case 57:
                                                            num134 = 377;
                                                            break;
                                                        case 58:
                                                            num134 = 300;
                                                            break;
                                                        case 59:
                                                            num134 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                                            {
                                                            364,
                                                            362
                                                            });
                                                            break;
                                                        case 60:
                                                            num134 = 148;
                                                            break;
                                                        case 61:
                                                            num134 = 361;
                                                            break;
                                                        case 62:
                                                            num134 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                                            {
                                                            487,
                                                            486,
                                                            485
                                                            });
                                                            break;
                                                        case 63:
                                                            num134 = 164;
                                                            flag11 &= NPC.MechSpawn((float)num131, (float)num132, 165);
                                                            break;
                                                        case 64:
                                                            num134 = 86;
                                                            flag12 = true;
                                                            break;
                                                        case 65:
                                                            num134 = 490;
                                                            break;
                                                        case 66:
                                                            num134 = 82;
                                                            break;
                                                        case 67:
                                                            num134 = 449;
                                                            break;
                                                        case 68:
                                                            num134 = 167;
                                                            break;
                                                        case 69:
                                                            num134 = 480;
                                                            break;
                                                        case 70:
                                                            num134 = 48;
                                                            break;
                                                        case 71:
                                                            num134 = (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                                            {
                                                            170,
                                                            180,
                                                            171
                                                            });
                                                            flag12 = true;
                                                            break;
                                                        case 72:
                                                            num134 = 481;
                                                            break;
                                                        case 73:
                                                            num134 = 482;
                                                            break;
                                                        case 74:
                                                            num134 = 430;
                                                            break;
                                                        case 75:
                                                            num134 = 489;
                                                            break;
                                                    }
                                                    if (num134 != -1 && CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, num134) && flag11) {
                                                        if (!flag12 || !Collision.SolidTiles(num129 - 2, num129 + 3, num128, num128 + 2)) {
                                                            num133 = NPC.NewNPC(num131, num132 - 12, num134, 0, 0f, 0f, 0f, 0f, 255);
                                                        } else {
                                                            Vector2 position = new Vector2((float)(num131 - 4), (float)(num132 - 22)) - new Vector2(10f);
                                                            Utils.PoofOfSmoke(position);
                                                            NetMessage.SendData(MessageID.PoofOfSmoke, -1, -1, null, (int)position.X, position.Y, 0f, 0f, 0, 0, 0);
                                                        }
                                                    }
                                                    if (num133 <= -1) {
                                                        if (num130 == 4) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 1)) {
                                                                num133 = NPC.NewNPC(num131, num132 - 12, 1, 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 7) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 49)) {
                                                                num133 = NPC.NewNPC(num131 - 4, num132 - 6, 49, 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 8) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 55)) {
                                                                num133 = NPC.NewNPC(num131, num132 - 12, 55, 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 9) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 46)) {
                                                                num133 = NPC.NewNPC(num131, num132 - 12, 46, 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 10) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 21)) {
                                                                num133 = NPC.NewNPC(num131, num132, 21, 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 18) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 67)) {
                                                                num133 = NPC.NewNPC(num131, num132 - 12, 67, 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 23) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 63)) {
                                                                num133 = NPC.NewNPC(num131, num132 - 12, 63, 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 27) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 85)) {
                                                                num133 = NPC.NewNPC(num131 - 9, num132, 85, 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 28) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 74)) {
                                                                num133 = NPC.NewNPC(num131, num132 - 12, (int)Utils.SelectRandom<short>(Main.rand, new short[]
                                                                {
                                                                    74,
                                                                    297,
                                                                    298
                                                                }), 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 34) {
                                                            for (int num135 = 0; num135 < 2; num135++) {
                                                                for (int num136 = 0; num136 < 3; num136++) {
                                                                    Tile tile22 = Main.tile[num129 + num135, num128 + num136];
                                                                    tile22.type = 349;
                                                                    tile22.frameX = (short)(num135 * 18 + 216);
                                                                    tile22.frameY = (short)(num136 * 18);
                                                                }
                                                            }
                                                            Animation.NewTemporaryAnimation(0, 349, num129, num128);
                                                            if (Main.netMode == NetmodeID.Server) {
                                                                NetMessage.SendTileRange(-1, num129, num128, 2, 3, TileChangeType.None);
                                                            }
                                                        } else if (num130 == 42) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 58)) {
                                                                num133 = NPC.NewNPC(num131, num132 - 12, 58, 0, 0f, 0f, 0f, 0f, 255);
                                                            }
                                                        } else if (num130 == 37) {
                                                            if (CheckMech(num129, num128, 600) && Item.MechSpawn((float)num131, (float)num132, 58) && Item.MechSpawn((float)num131, (float)num132, 1734) && Item.MechSpawn((float)num131, (float)num132, 1867)) {
                                                                Item.NewItem(num131, num132 - 16, 0, 0, 58, 1, false, 0, false, false);
                                                            }
                                                        } else if (num130 == 50) {
                                                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn((float)num131, (float)num132, 65)) {
                                                                if (!Collision.SolidTiles(num129 - 2, num129 + 3, num128, num128 + 2)) {
                                                                    num133 = NPC.NewNPC(num131, num132 - 12, 65, 0, 0f, 0f, 0f, 0f, 255);
                                                                } else {
                                                                    Vector2 position2 = new Vector2((float)(num131 - 4), (float)(num132 - 22)) - new Vector2(10f);
                                                                    Utils.PoofOfSmoke(position2);
                                                                    NetMessage.SendData(MessageID.PoofOfSmoke, -1, -1, null, (int)position2.X, position2.Y, 0f, 0f, 0, 0, 0);
                                                                }
                                                            }
                                                        } else if (num130 == 2) {
                                                            if (CheckMech(num129, num128, 600) && Item.MechSpawn((float)num131, (float)num132, 184) && Item.MechSpawn((float)num131, (float)num132, 1735) && Item.MechSpawn((float)num131, (float)num132, 1868)) {
                                                                Item.NewItem(num131, num132 - 16, 0, 0, 184, 1, false, 0, false, false);
                                                            }
                                                        } else if (num130 == 17) {
                                                            if (CheckMech(num129, num128, 600) && Item.MechSpawn((float)num131, (float)num132, 166)) {
                                                                Item.NewItem(num131, num132 - 20, 0, 0, 166, 1, false, 0, false, false);
                                                            }
                                                        } else if (num130 == 40) {
                                                            if (CheckMech(num129, num128, 300)) {
                                                                List<int> array = new List<int>();
                                                                int num137 = 0;
                                                                for (int num138 = 0; num138 < 200; num138++) {
                                                                    bool vanillaCanGo = Main.npc[num138].type == NPCID.Merchant || Main.npc[num138].type == NPCID.ArmsDealer || Main.npc[num138].type == NPCID.Guide || Main.npc[num138].type == NPCID.Demolitionist || Main.npc[num138].type == NPCID.Clothier || Main.npc[num138].type == NPCID.GoblinTinkerer || Main.npc[num138].type == NPCID.Wizard || Main.npc[num138].type == NPCID.SantaClaus || Main.npc[num138].type == NPCID.Truffle || Main.npc[num138].type == NPCID.DyeTrader || Main.npc[num138].type == NPCID.Cyborg || Main.npc[num138].type == NPCID.Painter || Main.npc[num138].type == NPCID.WitchDoctor || Main.npc[num138].type == NPCID.Pirate || Main.npc[num138].type == NPCID.LightningBug || Main.npc[num138].type == NPCID.Angler || Main.npc[num138].type == NPCID.DD2Bartender;
                                                                    if (Main.npc[num138].active && NPCLoader.CanGoToStatue(Main.npc[num138], true, vanillaCanGo)) {
                                                                        array.Add(num138);
                                                                        num137++;
                                                                    }
                                                                }
                                                                if (num137 > 0) {
                                                                    int num139 = array[Main.rand.Next(num137)];
                                                                    Main.npc[num139].position.X = (float)(num131 - Main.npc[num139].width / 2);
                                                                    Main.npc[num139].position.Y = (float)(num132 - Main.npc[num139].height - 1);
                                                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num139, 0f, 0f, 0f, 0, 0, 0);
                                                                    NPCLoader.OnGoToStatue(Main.npc[num139], true);
                                                                }
                                                            }
                                                        } else if (num130 == 41 && CheckMech(num129, num128, 300)) {
                                                            List<int> array2 = new List<int>();
                                                            int num140 = 0;
                                                            for (int num141 = 0; num141 < 200; num141++) {
                                                                bool vanillaCanGo2 = Main.npc[num141].type == NPCID.Nurse || Main.npc[num141].type == NPCID.Dryad || Main.npc[num141].type == NPCID.Mechanic || Main.npc[num141].type == NPCID.Steampunker || Main.npc[num141].type == NPCID.PartyGirl || Main.npc[num141].type == NPCID.Stylist;
                                                                if (Main.npc[num141].active && NPCLoader.CanGoToStatue(Main.npc[num141], false, vanillaCanGo2)) {
                                                                    array2.Add(num141);
                                                                    num140++;
                                                                }
                                                            }
                                                            if (num140 > 0) {
                                                                int num142 = array2[Main.rand.Next(num140)];
                                                                Main.npc[num142].position.X = (float)(num131 - Main.npc[num142].width / 2);
                                                                Main.npc[num142].position.Y = (float)(num132 - Main.npc[num142].height - 1);
                                                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num142, 0f, 0f, 0f, 0, 0, 0);
                                                                NPCLoader.OnGoToStatue(Main.npc[num142], false);
                                                            }
                                                        }
                                                    }
                                                    if (num133 >= 0) {
                                                        Main.npc[num133].value = 0f;
                                                        Main.npc[num133].npcSlots = 0f;
                                                        Main.npc[num133].SpawnedFromStatue = true;
                                                        return;
                                                    }
                                                } else if (type == 349) {
                                                    int num143 = j - (int)(tile.frameY / 18);
                                                    int num144;
                                                    for (num144 = (int)(tile.frameX / 18); num144 >= 2; num144 -= 2) {
                                                    }
                                                    num144 = i - num144;
                                                    SkipWire(num144, num143);
                                                    SkipWire(num144, num143 + 1);
                                                    SkipWire(num144, num143 + 2);
                                                    SkipWire(num144 + 1, num143);
                                                    SkipWire(num144 + 1, num143 + 1);
                                                    SkipWire(num144 + 1, num143 + 2);
                                                    short num145;
                                                    if (Main.tile[num144, num143].frameX == 0) {
                                                        num145 = 216;
                                                    } else {
                                                        num145 = -216;
                                                    }
                                                    for (int num146 = 0; num146 < 2; num146++) {
                                                        for (int num147 = 0; num147 < 3; num147++) {
                                                            Tile tile23 = Main.tile[num144 + num146, num143 + num147];
                                                            tile23.frameX += num145;
                                                        }
                                                    }
                                                    if (Main.netMode == NetmodeID.Server) {
                                                        NetMessage.SendTileRange(-1, num144, num143, 2, 3, TileChangeType.None);
                                                    }
                                                    Animation.NewTemporaryAnimation((num145 > 0) ? 0 : 1, 349, num144, num143);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                TileLoader.HitWire(i, j, type);
            }
        }

        // Token: 0x06000765 RID: 1893 RVA: 0x00359ABC File Offset: 0x00357CBC
        private static void Teleport() {
            if (_teleport[0].X < _teleport[1].X + 3f && _teleport[0].X > _teleport[1].X - 3f && _teleport[0].Y > _teleport[1].Y - 3f && _teleport[0].Y < _teleport[1].Y) {
                return;
            }
            Rectangle[] array = new Rectangle[2];
            array[0].X = (int)(_teleport[0].X * 16f);
            array[0].Width = 48;
            array[0].Height = 48;
            array[0].Y = (int)(_teleport[0].Y * 16f - (float)array[0].Height);
            array[1].X = (int)(_teleport[1].X * 16f);
            array[1].Width = 48;
            array[1].Height = 48;
            array[1].Y = (int)(_teleport[1].Y * 16f - (float)array[1].Height);
            for (int i = 0; i < 2; i++) {
                Vector2 value = new Vector2((float)(array[1].X - array[0].X), (float)(array[1].Y - array[0].Y));
                if (i == 1) {
                    value = new Vector2((float)(array[0].X - array[1].X), (float)(array[0].Y - array[1].Y));
                }
                if (!Wiring.blockPlayerTeleportationForOneIteration) {
                    for (int j = 0; j < 255; j++) {
                        if (Main.player[j].active && !Main.player[j].dead && !Main.player[j].teleporting && array[i].Intersects(Main.player[j].getRect())) {
                            Vector2 vector = Main.player[j].position + value;
                            Main.player[j].teleporting = true;
                            if (Main.netMode == NetmodeID.Server) {
                                RemoteClient.CheckSection(j, vector, 1);
                            }
                            Main.player[j].Teleport(vector, 0, 0);
                            if (Main.netMode == NetmodeID.Server) {
                                NetMessage.SendData(MessageID.Teleport, -1, -1, null, 0, (float)j, vector.X, vector.Y, 0, 0, 0);
                            }
                        }
                    }
                }
                for (int k = 0; k < 200; k++) {
                    if (Main.npc[k].active && !Main.npc[k].teleporting && Main.npc[k].lifeMax > 5 && !Main.npc[k].boss && !Main.npc[k].noTileCollide) {
                        int type = Main.npc[k].type;
                        if (!NPCID.Sets.TeleportationImmune[type] && array[i].Intersects(Main.npc[k].getRect())) {
                            Main.npc[k].teleporting = true;
                            Main.npc[k].Teleport(Main.npc[k].position + value, 0, 0);
                        }
                    }
                }
            }
            for (int l = 0; l < 255; l++) {
                Main.player[l].teleporting = false;
            }
            for (int m = 0; m < 200; m++) {
                Main.npc[m].teleporting = false;
            }
        }

        // Token: 0x06000766 RID: 1894 RVA: 0x00359EC4 File Offset: 0x003580C4
        public static void DeActive(int i, int j) {
            if (!Main.tile[i, j].active()) {
                return;
            }
            bool flag = Main.tileSolid[(int)Main.tile[i, j].type] && !TileID.Sets.NotReallySolid[(int)Main.tile[i, j].type];
            ushort type = Main.tile[i, j].type;
            if (type == 314 || type - 386 <= 3) {
                flag = false;
            }
            if (!flag) {
                return;
            }
            if (Main.tile[i, j - 1].active() && (Main.tile[i, j - 1].type == 5 || TileID.Sets.BasicChest[(int)Main.tile[i, j - 1].type] || Main.tile[i, j - 1].type == 26 || Main.tile[i, j - 1].type == 77 || Main.tile[i, j - 1].type == 72 || Main.tile[i, j - 1].type == 88)) {
                return;
            }
            Main.tile[i, j].inActive(true);
            WorldGen.SquareTileFrame(i, j, false);
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
            }
        }

        // Token: 0x06000767 RID: 1895 RVA: 0x0035A018 File Offset: 0x00358218
        public static void ReActive(int i, int j) {
            Main.tile[i, j].inActive(false);
            WorldGen.SquareTileFrame(i, j, false);
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
            }
        }

        // Token: 0x06000768 RID: 1896 RVA: 0x0035A048 File Offset: 0x00358248
        public static void MassWireOperationInner(Point ps, Point pe, Vector2 dropPoint, bool dir, ref int wireCount, ref int actuatorCount) {
            Math.Abs(ps.X - pe.X);
            Math.Abs(ps.Y - pe.Y);
            int num = Math.Sign(pe.X - ps.X);
            int num2 = Math.Sign(pe.Y - ps.Y);
            WiresUI.Settings.MultiToolMode toolMode = WiresUI.Settings.ToolMode;
            Point pt = default(Point);
            bool flag = false;
            Item.StartCachingType(530);
            Item.StartCachingType(849);
            int num3;
            int num4;
            int num5;
            if (dir) {
                pt.X = ps.X;
                num3 = ps.Y;
                num4 = pe.Y;
                num5 = num2;
            } else {
                pt.Y = ps.Y;
                num3 = ps.X;
                num4 = pe.X;
                num5 = num;
            }
            int num6 = num3;
            while (num6 != num4 && !flag) {
                if (dir) {
                    pt.Y = num6;
                } else {
                    pt.X = num6;
                }
                bool? flag2 = MassWireOperationStep(pt, toolMode, ref wireCount, ref actuatorCount);
                if (flag2 != null && !flag2.Value) {
                    flag = true;
                    break;
                }
                num6 += num5;
            }
            if (dir) {
                pt.Y = pe.Y;
                num3 = ps.X;
                num4 = pe.X;
                num5 = num;
            } else {
                pt.X = pe.X;
                num3 = ps.Y;
                num4 = pe.Y;
                num5 = num2;
            }
            int num7 = num3;
            while (num7 != num4 && !flag) {
                if (!dir) {
                    pt.Y = num7;
                } else {
                    pt.X = num7;
                }
                bool? flag3 = MassWireOperationStep(pt, toolMode, ref wireCount, ref actuatorCount);
                if (flag3 != null && !flag3.Value) {
                    flag = true;
                    break;
                }
                num7 += num5;
            }
            if (!flag) {
                MassWireOperationStep(pe, toolMode, ref wireCount, ref actuatorCount);
            }
            Item.DropCache(dropPoint, Vector2.Zero, 530, true);
            Item.DropCache(dropPoint, Vector2.Zero, 849, true);
        }

        // Token: 0x06000769 RID: 1897 RVA: 0x0035A228 File Offset: 0x00358428
        public static bool? MassWireOperationStep(Point pt, WiresUI.Settings.MultiToolMode mode, ref int wiresLeftToConsume, ref int actuatorsLeftToConstume) {
            if (!WorldGen.InWorld(pt.X, pt.Y, 1)) {
                return null;
            }
            Tile tile = Main.tile[pt.X, pt.Y];
            if (tile == null) {
                return null;
            }
            if (!mode.HasFlag(WiresUI.Settings.MultiToolMode.Cutter)) {
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Red) && !tile.wire()) {
                    if (wiresLeftToConsume <= 0) {
                        return new bool?(false);
                    }
                    wiresLeftToConsume--;
                    WorldGen.PlaceWire(pt.X, pt.Y);
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 5, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Green) && !tile.wire3()) {
                    if (wiresLeftToConsume <= 0) {
                        return new bool?(false);
                    }
                    wiresLeftToConsume--;
                    WorldGen.PlaceWire3(pt.X, pt.Y);
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 12, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Blue) && !tile.wire2()) {
                    if (wiresLeftToConsume <= 0) {
                        return new bool?(false);
                    }
                    wiresLeftToConsume--;
                    WorldGen.PlaceWire2(pt.X, pt.Y);
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 10, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Yellow) && !tile.wire4()) {
                    if (wiresLeftToConsume <= 0) {
                        return new bool?(false);
                    }
                    wiresLeftToConsume--;
                    WorldGen.PlaceWire4(pt.X, pt.Y);
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 16, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Actuator) && !tile.actuator()) {
                    if (actuatorsLeftToConstume <= 0) {
                        return new bool?(false);
                    }
                    actuatorsLeftToConstume--;
                    WorldGen.PlaceActuator(pt.X, pt.Y);
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 8, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
            }
            if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Cutter)) {
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Red) && tile.wire() && WorldGen.KillWire(pt.X, pt.Y)) {
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 6, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Green) && tile.wire3() && WorldGen.KillWire3(pt.X, pt.Y)) {
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 13, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Blue) && tile.wire2() && WorldGen.KillWire2(pt.X, pt.Y)) {
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 11, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Yellow) && tile.wire4() && WorldGen.KillWire4(pt.X, pt.Y)) {
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 17, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
                if (mode.HasFlag(WiresUI.Settings.MultiToolMode.Actuator) && tile.actuator() && WorldGen.KillActuator(pt.X, pt.Y)) {
                    NetMessage.SendData(MessageID.TileChange, -1, -1, null, 9, (float)pt.X, (float)pt.Y, 0f, 0, 0, 0);
                }
            }
            return new bool?(true);
        }

        // Token: 0x04000C6C RID: 3180
        private const int MaxPump = 20;

        // Token: 0x04000C6D RID: 3181
        private const int MaxMech = 1000;

        // Token: 0x04000C70 RID: 3184
        private static Dictionary<Point16, bool> _wireSkip;

        // Token: 0x04000C71 RID: 3185
        public static DoubleStack<Point16> _wireList;

        // Token: 0x04000C72 RID: 3186
        public static DoubleStack<byte> _wireDirectionList;

        // Token: 0x04000C73 RID: 3187
        public static Dictionary<Point16, byte> _toProcess;

        // Token: 0x04000C74 RID: 3188
        private static Queue<Point16> _GatesCurrent;

        // Token: 0x04000C75 RID: 3189
        public static Queue<Point16> _LampsToCheck;

        // Token: 0x04000C76 RID: 3190
        public static Queue<Point16> _GatesNext;

        // Token: 0x04000C77 RID: 3191
        private static Dictionary<Point16, bool> _GatesDone;

        // Token: 0x04000C78 RID: 3192
        private static Dictionary<Point16, byte> _PixelBoxTriggers;

        // Token: 0x04000C79 RID: 3193
        public static Vector2[] _teleport;

        // Token: 0x04000C7A RID: 3194
        public static int[] _inPumpX;

        // Token: 0x04000C7B RID: 3195
        public static int[] _inPumpY;

        // Token: 0x04000C7C RID: 3196
        public static int _numInPump;

        // Token: 0x04000C7D RID: 3197
        public static int[] _outPumpX;

        // Token: 0x04000C7E RID: 3198
        public static int[] _outPumpY;

        // Token: 0x04000C7F RID: 3199
        public static int _numOutPump;

        // Token: 0x04000C80 RID: 3200
        private static int[] _mechX;

        // Token: 0x04000C81 RID: 3201
        private static int[] _mechY;

        // Token: 0x04000C82 RID: 3202
        private static int _numMechs;

        // Token: 0x04000C83 RID: 3203
        private static int[] _mechTime;

        // Token: 0x04000C84 RID: 3204
        public static int _currentWireColor;

        // Token: 0x04000C85 RID: 3205
        private static int CurrentUser = 254;
    }
}
