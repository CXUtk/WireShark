﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using static WireShark.WiringWarpper;

namespace WireShark
{
    public abstract class TileInfo
    {
        protected int i, j, type;

        protected Tile tile;

        private static Dictionary<int, Type> tileinfo = new Dictionary<int, Type>();

        static TileInfo()
        {
            foreach (var type in typeof(TileInfo).Assembly.GetTypes())
            {
                if (!typeof(TileInfo).IsAssignableFrom(type) || type.IsAbstract) continue;
                var name = type.Name;
                var id = int.Parse(name.Substring(4));
                tileinfo.Add(id, type);
            }
        }


        public static TileInfo CreateTileInfo(int x, int y)
        {
            if (!tileinfo.TryGetValue(Main.tile[x, y].type, out var t)) return null;
            var result = Activator.CreateInstance(t) as TileInfo;
            result.i = x;
            result.j = y;
            result.tile = Main.tile[x, y];
            result.type = result.tile.type;
            return result;


        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void HitWireInternal();

        public void HitWire()
        {
            if (tile.actuator())
                WiringWarpper.ActuateForced(i, j);
            //if (!TileLoader.PreHitWire(i, j, type)) return;
            HitWireInternal();
            //TileLoader.HitWire(i, j, type);
        }
    }

    public class Tile144 : TileInfo
    {
        protected override void HitWireInternal()
        {
            WiringWarpper.HitSwitch(i, j);
            WorldGen.SquareTileFrame(i, j, true);
            return;
        }
    }

    public class Tile421 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                if (!tile.actuator())
                {
                    tile.type = 422;
                    WorldGen.SquareTileFrame(i, j, true);

                }

                return;
            }
        }
    }

    public class Tile422 : TileInfo
    {
        protected override void HitWireInternal()
        {
            if (!tile.actuator())
            {
                tile.type = 421;
                WorldGen.SquareTileFrame(i, j, true);
            }

            return;
        }
    }

    public class Tile255 : Tile268
    {
        
    }

    public class Tile256 : Tile268
    {
        
    }

    public class Tile257 : Tile268
    {
        
    }

    public class Tile258 : Tile268
    {
        
    }

    public class Tile259 : Tile268
    {
        
    }

    public class Tile260 : Tile268
    {
        
    }

    public class Tile261 : Tile268
    {
        
    }

    public class Tile262 : Tile268
    {
        
    }

    public class Tile263 : Tile268
    {
        
    }

    public class Tile264 : Tile268
    {
        
    }

    public class Tile265 : Tile268
    {
        
    }

    public class Tile266 : Tile268
    {
        
    }

    public class Tile267 : Tile268
    {
        
    }

    public class Tile268 : TileInfo
    {
        protected override void HitWireInternal()
        {
            if (!tile.actuator())
            {
                if (type >= 262)
                {
                    Tile tile2 = tile;
                    tile2.type -= 7;
                }
                else
                {
                    Tile tile3 = tile;
                    tile3.type += 7;
                }

                WorldGen.SquareTileFrame(i, j, true);

                return;
            }

            return;
        }
    }

    public class Tile419 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                LogicGate lgate = WiringWarpper.onLogicLampChange[i, j];
                switch (tile.frameX)
                {
                    case 0:
                    {
                        if (lgate != null)
                        {
                            ++lgate.lampon;
                            if (!lgate.erroronly)
                                WiringWarpper._LampsToCheck.Enqueue(lgate);
                        }

                        tile.frameX = 18;
                        break;
                    }
                    case 18:
                    {
                        if (lgate != null)
                        {
                            --lgate.lampon;
                            if (!lgate.erroronly)
                                WiringWarpper._LampsToCheck.Enqueue(lgate);
                        }

                        tile.frameX = 0;
                        break;
                    }
                    default:
                    {
                        if (lgate != null)
                            WiringWarpper._LampsToCheck.Enqueue(lgate);
                        break;
                    }
                }

                return;
            }
        }
    }

    public class Tile406 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num2 = tile.frameX % 54 / 18;
                int num3 = tile.frameY % 54 / 18;
                int num4 = i - num2;
                int num5 = j - num3;
                int num6 = 54;
                if (Main.tile[num4, num5].frameY >= 108)
                {
                    num6 = -108;
                }

                for (int k = num4; k < num4 + 3; k++)
                {
                    for (int l = num5; l < num5 + 3; l++)
                    {
                        Main.tile[k, l].frameY = (short) (Main.tile[k, l].frameY + num6);
                    }
                }

                return;
            }
        }
    }

    public class Tile452 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num7 = tile.frameX % 54 / 18;
                int num8 = tile.frameY % 54 / 18;
                int num9 = i - num7;
                int num10 = j - num8;
                int num11 = 54;
                if (Main.tile[num9, num10].frameX >= 54)
                {
                    num11 = -54;
                }

                for (int m = num9; m < num9 + 3; m++)
                {
                    for (int n = num10; n < num10 + 3; n++)
                    {
                        Main.tile[m, n].frameX = (short) (Main.tile[m, n].frameX + num11);
                    }
                }

                return;
            }
        }
    }

    public class Tile411 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num12 = tile.frameX % 36 / 18;
                int num13 = tile.frameY % 36 / 18;
                int num14 = i - num12;
                int num15 = j - num13;
                int num16 = 36;
                if (Main.tile[num14, num15].frameX >= 36)
                {
                    num16 = -36;
                }

                for (int num17 = num14; num17 < num14 + 2; num17++)
                {
                    for (int num18 = num15; num18 < num15 + 2; num18++)
                    {
                        Main.tile[num17, num18].frameX = (short) (Main.tile[num17, num18].frameX + num16);
                    }
                }

                return;
            }
        }
    }

    public class Tile425 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num19 = tile.frameX % 36 / 18;
                int num20 = tile.frameY % 36 / 18;
                int num21 = i - num19;
                int num22 = j - num20;
                for (int num23 = num21; num23 < num21 + 2; num23++)
                {
                    for (int num24 = num22; num24 < num22 + 2; num24++)
                    {
                    }
                }

                if (!Main.AnnouncementBoxDisabled)
                {
                    Color pink = Color.Pink;
                    int num25 = Sign.ReadSign(num21, num22, false);
                    if (num25 != -1 && Main.sign[num25] != null &&
                        !string.IsNullOrWhiteSpace(Main.sign[num25].text))
                    {
                        if (Main.AnnouncementBoxRange == -1)
                        {
                            if (Main.netMode == NetmodeID.SinglePlayer)
                            {
                                Main.NewTextMultiline(Main.sign[num25].text, false, pink, 460);
                                return;
                            }

                            if (Main.netMode == NetmodeID.Server)
                            {
                                return;
                            }
                        }
                        else if (Main.netMode == NetmodeID.SinglePlayer)
                        {
                            if (Main.player[Main.myPlayer]
                                    .Distance(new Vector2(num21 * 16 + 16, num22 * 16 + 16)) <=
                                Main.AnnouncementBoxRange)
                            {
                                Main.NewTextMultiline(Main.sign[num25].text, false, pink, 460);
                                return;
                            }
                        }
                        else if (Main.netMode == NetmodeID.Server)
                        {
                            for (int num26 = 0; num26 < 255; num26++)
                            {
                                if (Main.player[num26].active &&
                                    Main.player[num26]
                                        .Distance(new Vector2(num21 * 16 + 16, num22 * 16 + 16)) <=
                                    Main.AnnouncementBoxRange)
                                {
                                }
                            }

                            return;
                        }
                    }
                }

                return;
            }
        }
    }

    public class Tile405 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num27 = tile.frameX % 54 / 18;
                int num28 = tile.frameY % 36 / 18;
                int num29 = i - num27;
                int num30 = j - num28;
                int num31 = 54;
                if (Main.tile[num29, num30].frameX >= 54)
                {
                    num31 = -54;
                }

                for (int num32 = num29; num32 < num29 + 3; num32++)
                {
                    for (int num33 = num30; num33 < num30 + 2; num33++)
                    {
                        Main.tile[num32, num33].frameX = (short) (Main.tile[num32, num33].frameX + num31);
                    }
                }

                return;
            }
        }
    }

    public class Tile209 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num34 = tile.frameX % 72 / 18;
                int num35 = tile.frameY % 54 / 18;
                int num36 = i - num34;
                int num37 = j - num35;
                int num38 = tile.frameY / 54;
                int num39 = tile.frameX / 72;
                int num40 = -1;
                if (num34 == 1 || num34 == 2)
                {
                    num40 = num35;
                }

                int num41 = 0;
                if (num34 == 3)
                {
                    num41 = -54;
                }

                if (num34 == 0)
                {
                    num41 = 54;
                }

                if (num38 >= 8 && num41 > 0)
                {
                    num41 = 0;
                }

                if (num38 == 0 && num41 < 0)
                {
                    num41 = 0;
                }

                bool flag = false;
                if (num41 != 0)
                {
                    for (int num42 = num36; num42 < num36 + 4; num42++)
                    {
                        for (int num43 = num37; num43 < num37 + 3; num43++)
                        {
                            Main.tile[num42, num43].frameY = (short) (Main.tile[num42, num43].frameY + num41);
                        }
                    }

                    flag = true;
                }

                if ((num39 == 3 || num39 == 4) && (num40 == 0 || num40 == 1))
                {
                    num41 = ((num39 == 3) ? 72 : -72);
                    for (int num44 = num36; num44 < num36 + 4; num44++)
                    {
                        for (int num45 = num37; num45 < num37 + 3; num45++)
                        {
                            Main.tile[num44, num45].frameX = (short) (Main.tile[num44, num45].frameX + num41);
                        }
                    }

                    flag = true;
                }

                if (flag)
                {
                }

                if (num40 != -1)
                {
                    bool flag2 = true;
                    if ((num39 == 3 || num39 == 4) && num40 < 2)
                    {
                        flag2 = false;
                    }

                    if (WiringWarpper.CheckMech(num36, num37, 30) && flag2)
                    {
                        WorldGen.ShootFromCannon(num36, num37, num38, num39 + 1, 0, 0f, CurrentUser);
                        return;
                    }
                }

                return;
            }
        }
    }

    public class Tile212 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num46 = tile.frameX % 54 / 18;
                int num47 = tile.frameY % 54 / 18;
                int num48 = i - num46;
                int num49 = j - num47;
                short num148 = (short) (tile.frameX / 54);
                int num50 = -1;
                if (num46 == 1)
                {
                    num50 = num47;
                }

                int num51 = 0;
                if (num46 == 0)
                {
                    num51 = -54;
                }

                if (num46 == 2)
                {
                    num51 = 54;
                }

                if (num148 >= 1 && num51 > 0)
                {
                    num51 = 0;
                }

                if (num148 == 0 && num51 < 0)
                {
                    num51 = 0;
                }

                bool flag3 = false;
                if (num51 != 0)
                {
                    for (int num52 = num48; num52 < num48 + 3; num52++)
                    {
                        for (int num53 = num49; num53 < num49 + 3; num53++)
                        {
                            Main.tile[num52, num53].frameX = (short) (Main.tile[num52, num53].frameX + num51);
                        }
                    }

                    flag3 = true;
                }

                if (flag3)
                {
                }

                if (num50 != -1 && CheckMech(num48, num49, 10))
                {
                    float num149 = 12f + Main.rand.Next(450) * 0.01f;
                    float num54 = Main.rand.Next(85, 105);
                    float num150 = Main.rand.Next(-35, 11);
                    int type2 = 166;
                    int damage = 0;
                    float knockBack = 0f;
                    Vector2 vector = new Vector2((num48 + 2) * 16 - 8, (num49 + 2) * 16 - 8);
                    if (tile.frameX / 54 == 0)
                    {
                        num54 *= -1f;
                        vector.X -= 12f;
                    }
                    else
                    {
                        vector.X += 12f;
                    }

                    float num55 = num54;
                    float num56 = num150;
                    float num57 = (float) Math.Sqrt(num55 * num55 + num56 * num56);
                    num57 = num149 / num57;
                    num55 *= num57;
                    num56 *= num57;
                    Projectile.NewProjectile(vector.X, vector.Y, num55, num56, type2, damage, knockBack,
                        CurrentUser, 0f, 0f);
                    return;
                }

                return;
            }
        }
    }

    public class Tile215 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num58 = tile.frameX % 54 / 18;
                int num59 = tile.frameY % 36 / 18;
                int num60 = i - num58;
                int num61 = j - num59;
                int num62 = 36;
                if (Main.tile[num60, num61].frameY >= 36)
                {
                    num62 = -36;
                }

                for (int num63 = num60; num63 < num60 + 3; num63++)
                {
                    for (int num64 = num61; num64 < num61 + 2; num64++)
                    {
                        Main.tile[num63, num64].frameY = (short) (Main.tile[num63, num64].frameY + num62);
                    }
                }

                return;
            }
        }
    }

    public class Tile130 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                if (Main.tile[i, j - 1] != null && Main.tile[i, j - 1].active())
                {
                    if (TileID.Sets.BasicChest[Main.tile[i, j - 1].type] ||
                        TileID.Sets.BasicChestFake[Main.tile[i, j - 1].type])
                    {
                        return;
                    }

                    if (Main.tile[i, j - 1].type == 88)
                    {
                        return;
                    }
                }

                tile.type = 131;
                WorldGen.SquareTileFrame(i, j, true);

                return;
            }
        }
    }

    public class Tile131 : TileInfo
    {
        protected override void HitWireInternal()
        {

            tile.type = 130;
            WorldGen.SquareTileFrame(i, j, true);

            return;
        }
    }

    public class Tile387 : Tile386
    {
        
    }

    public class Tile386 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                bool value = type == 387;
                int num65 = WorldGen.ShiftTrapdoor(i, j, true, -1).ToInt();
                if (num65 == 0)
                {
                    num65 = -WorldGen.ShiftTrapdoor(i, j, false, -1).ToInt();
                }

                if (num65 != 0)
                {
                    return;
                }

                return;
            }
        }
    }

    public class Tile216 : TileInfo
    {
        protected override void HitWireInternal()
        {
            WorldGen.LaunchRocket(i, j);

            return;
        }
    }

    public class Tile335 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num67 = j - tile.frameY / 18;
                int num68 = i - tile.frameX / 18;


                if (CheckMech(num68, num67, 30))
                {
                    WorldGen.LaunchRocketSmall(num68, num67);
                    return;
                }

                return;
            }
        }
    }

    public class Tile338 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num69 = j - tile.frameY / 18;
                int num70 = i - tile.frameX / 18;


                if (CheckMech(num70, num69, 30))
                {
                    bool flag5 = false;
                    for (int num71 = 0; num71 < 1000; num71++)
                    {
                        if (Main.projectile[num71].active && Main.projectile[num71].aiStyle == 73 &&
                            Main.projectile[num71].ai[0] == num70 && Main.projectile[num71].ai[1] == num69)
                        {
                            flag5 = true;
                            break;
                        }
                    }

                    if (!flag5)
                    {
                        Projectile.NewProjectile(num70 * 16 + 8, num69 * 16 + 2, 0f, 0f,
                            419 + Main.rand.Next(4), 0, 0f, Main.myPlayer, num70, num69);
                        return;
                    }
                }

                return;
            }
        }
    }

    public class Tile235 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num72 = i - tile.frameX / 18;
                if (tile.wall == 87 && j > Main.worldSurface && !NPC.downedPlantBoss)
                {
                    return;
                }

                if (_teleport[0].X == -1f)
                {
                    _teleport[0].X = num72;
                    _teleport[0].Y = j;
                    if (tile.halfBrick())
                    {
                        Vector2[] expr_EFC_cp_0 = _teleport;
                        int expr_EFC_cp_ = 0;
                        expr_EFC_cp_0[expr_EFC_cp_].Y = expr_EFC_cp_0[expr_EFC_cp_].Y + 0.5f;
                        return;
                    }
                }
                else if (_teleport[0].X != num72 || _teleport[0].Y != j)
                {
                    _teleport[1].X = num72;
                    _teleport[1].Y = j;
                    if (tile.halfBrick())
                    {
                        Vector2[] expr_F75_cp_0 = _teleport;
                        int expr_F75_cp_ = 1;
                        expr_F75_cp_0[expr_F75_cp_].Y = expr_F75_cp_0[expr_F75_cp_].Y + 0.5f;
                        return;
                    }
                }

                return;
            }
        }
    }

    public class Tile429 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                short num151 = (short) (Main.tile[i, j].frameX / 18);
                bool flag6 = num151 % 2 >= 1;
                bool flag7 = num151 % 4 >= 2;
                bool flag8 = num151 % 8 >= 4;
                bool flag9 = num151 % 16 >= 8;
                bool flag10 = false;
                short num73 = 0;
                switch (_currentWireColor)
                {
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

                if (flag10)
                {
                    Tile tile6 = tile;
                    tile6.frameX += num73;
                }
                else
                {
                    Tile tile7 = tile;
                    tile7.frameX -= num73;
                }

                return;
            }
        }
    }

    public class Tile149 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                if (tile.frameX < 54)
                {
                    Tile tile8 = tile;
                    tile8.frameX += 54;
                }
                else
                {
                    Tile tile9 = tile;
                    tile9.frameX -= 54;
                }

                return;
            }
        }
    }

    public class Tile244 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num74;
                for (num74 = tile.frameX / 18; num74 >= 3; num74 -= 3)
                {
                }

                int num75;
                for (num75 = tile.frameY / 18; num75 >= 3; num75 -= 3)
                {
                }

                int num76 = i - num74;
                int num77 = j - num75;
                int num78 = 54;
                if (Main.tile[num76, num77].frameX >= 54)
                {
                    num78 = -54;
                }

                for (int num79 = num76; num79 < num76 + 3; num79++)
                {
                    for (int num80 = num77; num80 < num77 + 2; num80++)
                    {
                        Main.tile[num79, num80].frameX = (short) (Main.tile[num79, num80].frameX + num78);
                    }
                }

                return;
            }
        }
    }

    public class Tile42 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num81;
                for (num81 = tile.frameY / 18; num81 >= 2; num81 -= 2)
                {
                }

                int num82 = j - num81;
                short num83 = 18;
                if (tile.frameX > 0)
                {
                    num83 = -18;
                }

                Tile tile10 = Main.tile[i, num82];
                tile10.frameX += num83;
                Tile tile11 = Main.tile[i, num82 + 1];
                tile11.frameX += num83;


                return;
            }
        }
    }

    public class Tile93 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num84;
                for (num84 = tile.frameY / 18; num84 >= 3; num84 -= 3)
                {
                }

                num84 = j - num84;
                short num85 = 18;
                if (tile.frameX > 0)
                {
                    num85 = -18;
                }

                Tile tile12 = Main.tile[i, num84];
                tile12.frameX += num85;
                Tile tile13 = Main.tile[i, num84 + 1];
                tile13.frameX += num85;
                Tile tile14 = Main.tile[i, num84 + 2];
                tile14.frameX += num85;


                return;
            }
        }
    }

    public class Tile126 : Tile173
    {
        
    }

    public class Tile95 : Tile173
    {
        
    }

    public class Tile100 : Tile173
    {
        
    }

    public class Tile173 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num86;
                for (num86 = tile.frameY / 18; num86 >= 2; num86 -= 2)
                {
                }

                num86 = j - num86;
                int num87 = tile.frameX / 18;
                if (num87 > 1)
                {
                    num87 -= 2;
                }

                num87 = i - num87;
                short num88 = 36;
                if (Main.tile[num87, num86].frameX > 0)
                {
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


                return;
            }
        }
    }

    public class Tile34 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num89;
                for (num89 = tile.frameY / 18; num89 >= 3; num89 -= 3)
                {
                }

                int num90 = j - num89;
                int num91 = tile.frameX % 108 / 18;
                if (num91 > 2)
                {
                    num91 -= 3;
                }

                num91 = i - num91;
                short num92 = 54;
                if (Main.tile[num91, num90].frameX % 108 > 0)
                {
                    num92 = -54;
                }

                for (int num93 = num91; num93 < num91 + 3; num93++)
                {
                    for (int num94 = num90; num94 < num90 + 3; num94++)
                    {
                        Tile tile19 = Main.tile[num93, num94];
                        tile19.frameX += num92;
                    }
                }

                return;
            }
        }
    }

    public class Tile314 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                if (CheckMech(i, j, 5))
                {
                    Minecart.FlipSwitchTrack(i, j);
                    return;
                }

                return;
            }
        }
    }

    public class Tile389 : Tile388
    {
        
    }

    public class Tile388 : TileInfo
    {
        protected override void HitWireInternal()
        {
            bool flag4 = type == 389;
            WorldGen.ShiftTallGate(i, j, flag4);

            return;
        }
    }

    public class Tile33 : Tile174
    {
        
    }

    public class Tile174 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                short num95 = 18;
                if (tile.frameX > 0)
                {
                    num95 = -18;
                }

                Tile tile20 = tile;
                tile20.frameX += num95;

                return;
            }
        }
    }

    public class Tile92 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num96 = j - tile.frameY / 18;
                short num97 = 18;
                if (tile.frameX > 0)
                {
                    num97 = -18;
                }

                for (int num98 = num96; num98 < num96 + 6; num98++)
                {
                    Tile tile21 = Main.tile[i, num98];
                    tile21.frameX += num97;
                }

                return;
            }
        }
    }

    public class Tile137 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num99 = tile.frameY / 18;
                Vector2 zero = Vector2.Zero;
                float speedX = 0f;
                float speedY = 0f;
                int num100 = 0;
                int damage2 = 0;
                switch (num99)
                {
                    case 0:
                    case 1:
                    case 2:
                        if (CheckMech(i, j, 200))
                        {
                            int num101 = (tile.frameX == 0) ? -1 : ((tile.frameX == 18) ? 1 : 0);
                            int num102 = (tile.frameX < 36) ? 0 : ((tile.frameX < 72) ? -1 : 1);
                            zero = new Vector2(i * 16 + 8 + 10 * num101, j * 16 + 9 + num102 * 9);
                            float num103 = 3f;
                            if (num99 == 0)
                            {
                                num100 = 98;
                                damage2 = 20;
                                num103 = 12f;
                            }

                            if (num99 == 1)
                            {
                                num100 = 184;
                                damage2 = 40;
                                num103 = 12f;
                            }

                            if (num99 == 2)
                            {
                                num100 = 187;
                                damage2 = 40;
                                num103 = 5f;
                            }

                            speedX = num101 * num103;
                            speedY = num102 * num103;
                        }

                        break;
                    case 3:
                        if (CheckMech(i, j, 300))
                        {
                            int num104 = 200;
                            for (int num105 = 0; num105 < 1000; num105++)
                            {
                                if (Main.projectile[num105].active && Main.projectile[num105].type == num100)
                                {
                                    float num106 = (new Vector2(i * 16 + 8, j * 18 + 8) -
                                                    Main.projectile[num105].Center).Length();
                                    if (num106 < 50f)
                                    {
                                        num104 -= 50;
                                    }
                                    else if (num106 < 100f)
                                    {
                                        num104 -= 15;
                                    }
                                    else if (num106 < 200f)
                                    {
                                        num104 -= 10;
                                    }
                                    else if (num106 < 300f)
                                    {
                                        num104 -= 8;
                                    }
                                    else if (num106 < 400f)
                                    {
                                        num104 -= 6;
                                    }
                                    else if (num106 < 500f)
                                    {
                                        num104 -= 5;
                                    }
                                    else if (num106 < 700f)
                                    {
                                        num104 -= 4;
                                    }
                                    else if (num106 < 900f)
                                    {
                                        num104 -= 3;
                                    }
                                    else if (num106 < 1200f)
                                    {
                                        num104 -= 2;
                                    }
                                    else
                                    {
                                        num104--;
                                    }
                                }
                            }

                            if (num104 > 0)
                            {
                                num100 = 185;
                                damage2 = 40;
                                int num107 = 0;
                                int num108 = 0;
                                switch (tile.frameX / 18)
                                {
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

                                speedX = 4 * num107 + Main.rand.Next(-20 + ((num107 == 1) ? 20 : 0),
                                    21 - ((num107 == -1) ? 20 : 0)) * 0.05f;
                                speedY = 4 * num108 + Main.rand.Next(-20 + ((num108 == 1) ? 20 : 0),
                                    21 - ((num108 == -1) ? 20 : 0)) * 0.05f;
                                zero = new Vector2(i * 16 + 8 + 14 * num107, j * 16 + 8 + 14 * num108);
                            }
                        }

                        break;
                    case 4:
                        if (CheckMech(i, j, 90))
                        {
                            int num109 = 0;
                            int num110 = 0;
                            switch (tile.frameX / 18)
                            {
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

                            speedX = 8 * num109;
                            speedY = 8 * num110;
                            damage2 = 60;
                            num100 = 186;
                            zero = new Vector2(i * 16 + 8 + 18 * num109, j * 16 + 8 + 18 * num110);
                        }

                        break;
                }

                switch (num99 + 10)
                {
                    case 0:
                        if (CheckMech(i, j, 200))
                        {
                            int num111 = -1;
                            if (tile.frameX != 0)
                            {
                                num111 = 1;
                            }

                            speedX = 12 * num111;
                            damage2 = 20;
                            num100 = 98;
                            zero = new Vector2(i * 16 + 8, j * 16 + 7);
                            zero.X += 10 * num111;
                            zero.Y += 2f;
                        }

                        break;
                    case 1:
                        if (CheckMech(i, j, 200))
                        {
                            int num112 = -1;
                            if (tile.frameX != 0)
                            {
                                num112 = 1;
                            }

                            speedX = 12 * num112;
                            damage2 = 40;
                            num100 = 184;
                            zero = new Vector2(i * 16 + 8, j * 16 + 7);
                            zero.X += 10 * num112;
                            zero.Y += 2f;
                        }

                        break;
                    case 2:
                        if (CheckMech(i, j, 200))
                        {
                            int num113 = -1;
                            if (tile.frameX != 0)
                            {
                                num113 = 1;
                            }

                            speedX = 5 * num113;
                            damage2 = 40;
                            num100 = 187;
                            zero = new Vector2(i * 16 + 8, j * 16 + 7);
                            zero.X += 10 * num113;
                            zero.Y += 2f;
                        }

                        break;
                    case 3:
                        if (CheckMech(i, j, 300))
                        {
                            num100 = 185;
                            int num114 = 200;
                            for (int num115 = 0; num115 < 1000; num115++)
                            {
                                if (Main.projectile[num115].active && Main.projectile[num115].type == num100)
                                {
                                    float num116 = (new Vector2(i * 16 + 8, j * 18 + 8) -
                                                    Main.projectile[num115].Center).Length();
                                    if (num116 < 50f)
                                    {
                                        num114 -= 50;
                                    }
                                    else if (num116 < 100f)
                                    {
                                        num114 -= 15;
                                    }
                                    else if (num116 < 200f)
                                    {
                                        num114 -= 10;
                                    }
                                    else if (num116 < 300f)
                                    {
                                        num114 -= 8;
                                    }
                                    else if (num116 < 400f)
                                    {
                                        num114 -= 6;
                                    }
                                    else if (num116 < 500f)
                                    {
                                        num114 -= 5;
                                    }
                                    else if (num116 < 700f)
                                    {
                                        num114 -= 4;
                                    }
                                    else if (num116 < 900f)
                                    {
                                        num114 -= 3;
                                    }
                                    else if (num116 < 1200f)
                                    {
                                        num114 -= 2;
                                    }
                                    else
                                    {
                                        num114--;
                                    }
                                }
                            }

                            if (num114 > 0)
                            {
                                speedX = Main.rand.Next(-20, 21) * 0.05f;
                                speedY = 4f + Main.rand.Next(0, 21) * 0.05f;
                                damage2 = 40;
                                zero = new Vector2(i * 16 + 8, j * 16 + 16);
                                zero.Y += 6f;
                                Projectile.NewProjectile((int) zero.X, (int) zero.Y, speedX, speedY, num100,
                                    damage2, 2f, Main.myPlayer, 0f, 0f);
                            }
                        }

                        break;
                    case 4:
                        if (CheckMech(i, j, 90))
                        {
                            speedX = 0f;
                            speedY = 8f;
                            damage2 = 60;
                            num100 = 186;
                            zero = new Vector2(i * 16 + 8, j * 16 + 16);
                            zero.Y += 10f;
                        }

                        break;
                }

                if (num100 != 0)
                {
                    Projectile.NewProjectile((int) zero.X, (int) zero.Y, speedX, speedY, num100, damage2, 2f,
                        Main.myPlayer, 0f, 0f);
                    return;
                }

                return;
            }
        }
    }

    public class Tile443 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num117 = tile.frameX / 36;
                int num118 = i - (tile.frameX - num117 * 36) / 18;
                if (CheckMech(num118, j, 200))
                {
                    Vector2 vector2 = Vector2.Zero;
                    Vector2 zero2 = Vector2.Zero;
                    int num119 = 654;
                    int damage3 = 20;
                    if (num117 < 2)
                    {
                        vector2 = new Vector2(num118 + 1, j) * 16f;
                        zero2 = new Vector2(0f, -8f);
                    }
                    else
                    {
                        vector2 = new Vector2(num118 + 1, j + 1) * 16f;
                        zero2 = new Vector2(0f, 8f);
                    }

                    if (num119 != 0)
                    {
                        Projectile.NewProjectile((int) vector2.X, (int) vector2.Y, zero2.X, zero2.Y, num119,
                            damage3, 2f, Main.myPlayer, 0f, 0f);
                        return;
                    }
                }

                return;
            }
        }
    }

    public class Tile207 : TileInfo
    {
        protected override void HitWireInternal()
        {
            WorldGen.SwitchFountain(i, j);
            return;
        }
    }

    public class Tile410 : TileInfo
    {
        protected override void HitWireInternal()
        {
            WorldGen.SwitchMonolith(i, j);
            return;
        }
    }

    public class Tile455 : TileInfo
    {
        protected override void HitWireInternal()
        {
            BirthdayParty.ToggleManualParty();
            return;
        }
    }

    public class Tile141 : TileInfo
    {
        protected override void HitWireInternal()
        {
            WorldGen.KillTile(i, j, false, false, true);

            Projectile.NewProjectile(i * 16 + 8, j * 16 + 8, 0f, 0f, ProjectileID.Explosives, 500, 10f,
                Main.myPlayer, 0f, 0f);
            return;
        }
    }

    public class Tile210 : TileInfo
    {
        protected override void HitWireInternal()
        {
            WorldGen.ExplodeMine(i, j);
            return;
        }
    }

    public class Tile142 : Tile143
    {
        
    }

    public class Tile143 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num120 = j - tile.frameY / 18;
                int num121 = tile.frameX / 18;
                if (num121 > 1)
                {
                    num121 -= 2;
                }

                num121 = i - num121;


                if (type == 142)
                {
                    for (int num122 = 0; num122 < 4; num122++)
                    {
                        if (_numInPump >= 19)
                        {
                            return;
                        }

                        int num123;
                        int num124;
                        if (num122 == 0)
                        {
                            num123 = num121;
                            num124 = num120 + 1;
                        }
                        else if (num122 == 1)
                        {
                            num123 = num121 + 1;
                            num124 = num120 + 1;
                        }
                        else if (num122 == 2)
                        {
                            num123 = num121;
                            num124 = num120;
                        }
                        else
                        {
                            num123 = num121 + 1;
                            num124 = num120;
                        }

                        _inPumpX[_numInPump] = num123;
                        _inPumpY[_numInPump] = num124;
                        _numInPump++;
                    }

                    return;
                }

                for (int num125 = 0; num125 < 4; num125++)
                {
                    if (_numOutPump >= 19)
                    {
                        return;
                    }

                    int num126;
                    int num127;
                    if (num125 == 0)
                    {
                        num126 = num121;
                        num127 = num120 + 1;
                    }
                    else if (num125 == 1)
                    {
                        num126 = num121 + 1;
                        num127 = num120 + 1;
                    }
                    else if (num125 == 2)
                    {
                        num126 = num121;
                        num127 = num120;
                    }
                    else
                    {
                        num126 = num121 + 1;
                        num127 = num120;
                    }

                    _outPumpX[_numOutPump] = num126;
                    _outPumpY[_numOutPump] = num127;
                    _numOutPump++;
                }

                return;
            }
        }
    }

    public class Tile105 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num128 = j - tile.frameY / 18;
                int num129 = tile.frameX / 18;
                int num130 = 0;
                while (num129 >= 2)
                {
                    num129 -= 2;
                    num130++;
                }

                num129 = i - num129;
                num129 = i - tile.frameX % 36 / 18;
                num128 = j - tile.frameY % 54 / 18;
                num130 = tile.frameX / 36 + tile.frameY / 54 * 55;


                int num131 = num129 * 16 + 16;
                int num132 = (num128 + 3) * 16;
                int num133 = -1;
                int num134 = -1;
                bool flag11 = true;
                bool flag12 = false;
                switch (num130)
                {
                    case 51:
                        num134 = Utils.SelectRandom<short>(Main.rand, new short[]
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
                        num134 = Utils.SelectRandom<short>(Main.rand, new short[]
                        {
                            355,
                            358
                        });
                        break;
                    case 55:
                        num134 = Utils.SelectRandom<short>(Main.rand, new short[]
                        {
                            367,
                            366
                        });
                        break;
                    case 56:
                        num134 = Utils.SelectRandom<short>(Main.rand, new short[]
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
                        num134 = Utils.SelectRandom<short>(Main.rand, new short[]
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
                        num134 = Utils.SelectRandom<short>(Main.rand, new short[]
                        {
                            487,
                            486,
                            485
                        });
                        break;
                    case 63:
                        num134 = 164;
                        flag11 &= NPC.MechSpawn(num131, num132, 165);
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
                        num134 = Utils.SelectRandom<short>(Main.rand, new short[]
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

                if (num134 != -1 && CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, num134) &&
                    flag11)
                {
                    if (!flag12 || !Collision.SolidTiles(num129 - 2, num129 + 3, num128, num128 + 2))
                    {
                        num133 = NPC.NewNPC(num131, num132 - 12, num134, 0, 0f, 0f, 0f, 0f, 255);
                    }
                    else
                    {
                        Vector2 position = new Vector2(num131 - 4, num132 - 22) - new Vector2(10f);
                        Utils.PoofOfSmoke(position);
                    }
                }

                if (num133 <= -1)
                {
                    switch (num130)
                    {
                        case 4:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 1))
                            {
                                num133 = NPC.NewNPC(num131, num132 - 12, 1, 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 7:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 49))
                            {
                                num133 = NPC.NewNPC(num131 - 4, num132 - 6, 49, 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 8:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 55))
                            {
                                num133 = NPC.NewNPC(num131, num132 - 12, 55, 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 9:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 46))
                            {
                                num133 = NPC.NewNPC(num131, num132 - 12, 46, 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 10:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 21))
                            {
                                num133 = NPC.NewNPC(num131, num132, 21, 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 18:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 67))
                            {
                                num133 = NPC.NewNPC(num131, num132 - 12, 67, 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 23:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 63))
                            {
                                num133 = NPC.NewNPC(num131, num132 - 12, 63, 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 27:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 85))
                            {
                                num133 = NPC.NewNPC(num131 - 9, num132, 85, 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 28:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 74))
                            {
                                num133 = NPC.NewNPC(num131, num132 - 12, Utils.SelectRandom<short>(Main.rand,
                                    new short[]
                                    {
                                        74,
                                        297,
                                        298
                                    }), 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 34:
                        {
                            for (int num135 = 0; num135 < 2; num135++)
                            {
                                for (int num136 = 0; num136 < 3; num136++)
                                {
                                    Tile tile22 = Main.tile[num129 + num135, num128 + num136];
                                    tile22.type = 349;
                                    tile22.frameX = (short) (num135 * 18 + 216);
                                    tile22.frameY = (short) (num136 * 18);
                                }
                            }

                            Animation.NewTemporaryAnimation(0, 349, num129, num128);
                            if (Main.netMode == NetmodeID.Server)
                            {
                            }

                            break;
                        }
                        case 42:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 58))
                            {
                                num133 = NPC.NewNPC(num131, num132 - 12, 58, 0, 0f, 0f, 0f, 0f, 255);
                            }

                            break;
                        }
                        case 37:
                        {
                            if (CheckMech(num129, num128, 600) && Item.MechSpawn(num131, num132, 58) &&
                                Item.MechSpawn(num131, num132, 1734) && Item.MechSpawn(num131, num132, 1867))
                            {
                                Item.NewItem(num131, num132 - 16, 0, 0, 58, 1, false, 0, false, false);
                            }

                            break;
                        }
                        case 50:
                        {
                            if (CheckMech(num129, num128, 30) && NPC.MechSpawn(num131, num132, 65))
                            {
                                if (!Collision.SolidTiles(num129 - 2, num129 + 3, num128, num128 + 2))
                                {
                                    num133 = NPC.NewNPC(num131, num132 - 12, 65, 0, 0f, 0f, 0f, 0f, 255);
                                }
                                else
                                {
                                    Vector2 position2 = new Vector2(num131 - 4, num132 - 22) - new Vector2(10f);
                                    Utils.PoofOfSmoke(position2);
                                }
                            }

                            break;
                        }
                        case 2:
                        {
                            if (CheckMech(num129, num128, 600) && Item.MechSpawn(num131, num132, 184) &&
                                Item.MechSpawn(num131, num132, 1735) && Item.MechSpawn(num131, num132, 1868))
                            {
                                Item.NewItem(num131, num132 - 16, 0, 0, 184, 1, false, 0, false, false);
                            }

                            break;
                        }
                        case 17:
                        {
                            if (CheckMech(num129, num128, 600) && Item.MechSpawn(num131, num132, 166))
                            {
                                Item.NewItem(num131, num132 - 20, 0, 0, 166, 1, false, 0, false, false);
                            }

                            break;
                        }
                        case 40:
                        {
                            if (CheckMech(num129, num128, 300))
                            {
                                List<int> array = new List<int>();
                                int num137 = 0;
                                for (int num138 = 0; num138 < 200; num138++)
                                {
                                    bool vanillaCanGo = Main.npc[num138].type == NPCID.Merchant ||
                                                        Main.npc[num138].type == NPCID.ArmsDealer ||
                                                        Main.npc[num138].type == NPCID.Guide ||
                                                        Main.npc[num138].type == NPCID.Demolitionist ||
                                                        Main.npc[num138].type == NPCID.Clothier ||
                                                        Main.npc[num138].type == NPCID.GoblinTinkerer ||
                                                        Main.npc[num138].type == NPCID.Wizard ||
                                                        Main.npc[num138].type == NPCID.SantaClaus ||
                                                        Main.npc[num138].type == NPCID.Truffle ||
                                                        Main.npc[num138].type == NPCID.DyeTrader ||
                                                        Main.npc[num138].type == NPCID.Cyborg ||
                                                        Main.npc[num138].type == NPCID.Painter ||
                                                        Main.npc[num138].type == NPCID.WitchDoctor ||
                                                        Main.npc[num138].type == NPCID.Pirate ||
                                                        Main.npc[num138].type == NPCID.LightningBug ||
                                                        Main.npc[num138].type == NPCID.Angler ||
                                                        Main.npc[num138].type == NPCID.DD2Bartender;
                                    if (Main.npc[num138].active &&
                                        NPCLoader.CanGoToStatue(Main.npc[num138], true, vanillaCanGo))
                                    {
                                        array.Add(num138);
                                        num137++;
                                    }
                                }

                                if (num137 > 0)
                                {
                                    int num139 = array[Main.rand.Next(num137)];
                                    Main.npc[num139].position.X = num131 - Main.npc[num139].width / 2;
                                    Main.npc[num139].position.Y = num132 - Main.npc[num139].height - 1;

                                    NPCLoader.OnGoToStatue(Main.npc[num139], true);
                                }
                            }

                            break;
                        }
                        case 41:
                        {
                            if (CheckMech(num129, num128, 300))
                            {
                                List<int> array2 = new List<int>();
                                int num140 = 0;
                                for (int num141 = 0; num141 < 200; num141++)
                                {
                                    bool vanillaCanGo2 = Main.npc[num141].type == NPCID.Nurse ||
                                                         Main.npc[num141].type == NPCID.Dryad ||
                                                         Main.npc[num141].type == NPCID.Mechanic ||
                                                         Main.npc[num141].type == NPCID.Steampunker ||
                                                         Main.npc[num141].type == NPCID.PartyGirl ||
                                                         Main.npc[num141].type == NPCID.Stylist;
                                    if (Main.npc[num141].active &&
                                        NPCLoader.CanGoToStatue(Main.npc[num141], false, vanillaCanGo2))
                                    {
                                        array2.Add(num141);
                                        num140++;
                                    }
                                }

                                if (num140 > 0)
                                {
                                    int num142 = array2[Main.rand.Next(num140)];
                                    Main.npc[num142].position.X = num131 - Main.npc[num142].width / 2;
                                    Main.npc[num142].position.Y = num132 - Main.npc[num142].height - 1;

                                    NPCLoader.OnGoToStatue(Main.npc[num142], false);
                                }

                            }

                            break;
                        }
                    }
                }

                if (num133 >= 0)
                {
                    Main.npc[num133].value = 0f;
                    Main.npc[num133].npcSlots = 0f;
                    Main.npc[num133].SpawnedFromStatue = true;
                    return;
                }

                return;
            }
        }
    }

    public class Tile349 : TileInfo
    {
        protected override void HitWireInternal()
        {
            {
                int num143 = j - tile.frameY / 18;
                int num144;
                for (num144 = tile.frameX / 18; num144 >= 2; num144 -= 2)
                {
                }

                num144 = i - num144;


                short num145;
                if (Main.tile[num144, num143].frameX == 0)
                {
                    num145 = 216;
                }
                else
                {
                    num145 = -216;
                }

                for (int num146 = 0; num146 < 2; num146++)
                {
                    for (int num147 = 0; num147 < 3; num147++)
                    {
                        Tile tile23 = Main.tile[num144 + num146, num143 + num147];
                        tile23.frameX += num145;
                    }
                }

                if (Main.netMode == NetmodeID.Server)
                {
                }

                Animation.NewTemporaryAnimation((num145 > 0) ? 0 : 1, 349, num144, num143);
                return;
            }
        }
    }

    public class Tile139 : Tile35
    {
        
    }

    public class Tile35 : TileInfo
    {
        protected override void HitWireInternal()
        {
            WorldGen.SwitchMB(i, j);
            return;
        }
    }
    public class Tile4 : TileInfo
    {
        protected override void HitWireInternal()
        {
            if (tile.frameX < 66)
            {
                tile.frameX += 66;
            }
            else
            {
                tile.frameX -= 66;
            }
        }
    }
}
/*
if (TileLoader.CloseDoorID(Main.tile[i, j]) >= 0)
{
    if (WorldGen.CloseDoor(i, j, true)) return;
}
else if (TileLoader.OpenDoorID(Main.tile[i, j]) >= 0)
{
    int num66 = 1;
    if (Main.rand.Next(2) == 0)
    {
        num66 = -1;
    }

    if (WorldGen.OpenDoor(i, j, num66))
    {
        return;
    }

    if (WorldGen.OpenDoor(i, j, -num66))
    {
        return;
    }
}
else if (TileLoader.IsTorch(type))
{
    if (tile.frameX < 66)
    {
        Tile tile4 = tile;
        tile4.frameX += 66;
    }
    else
    {
        Tile tile5 = tile;
        tile5.frameX -= 66;
    }

    return;
}
else if (TileLoader.IsModMusicBox(tile))
{
    WorldGen.SwitchMB(i, j);
    return;
}
*/
