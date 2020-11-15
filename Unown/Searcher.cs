using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Unown
{
    public partial class Searcher : Form
    {
        public uint tsv;
        public Searcher(uint tsv)
        {
            InitializeComponent();
        }

        public List<uint> Recover(uint hp, uint atk, uint def, uint spa, uint spd, uint spe)
        {
            uint add;
            uint k;
            uint mult;
            byte[] low = new byte[0x10000];
            bool[] flags = new bool[0x10000];

            k = 0xC64E6D00; // Mult << 8
            mult = 0x41c64e6d; // pokerng constant
            add = 0x6073; // pokerng constant
            uint count = 0;
            foreach (byte element in low)
            {
                low[count] = 0;
                count++;
            }
            count = 0;
            foreach (bool element in flags)
            {
                flags[count] = true;
                count++;
            }
            for (short i = 0; i < 256; i++)
            {
                uint right = (uint)(mult * i + add);
                ushort val = (ushort)(right >> 16);
                flags[val] = true;
                low[val--] = (byte)(i);
                flags[val] = true;
                low[val] = (byte)(i);
            }

            List<uint> origin = new List<uint>();
            uint first = (hp | (atk << 5) | (def << 10)) << 16;
            uint second = (spe | (spa << 5) | (spd << 10)) << 16;

            uint search1 = second - first * mult;
            uint search2 = second - (first ^ 0x80000000);

            for (uint i = 0; i < 256; i++, search1 -= k, search2 -= k)
            {
                if (flags[search1 >> 16])
                {
                    uint test = first | (i << 8) | low[search1 >> 16];
                    if (((test * mult + add) & 0x7fff0000) == second)
                    {
                        origin.Add(test);
                    }

                }
                if (flags[search2 >> 16])
                {
                    uint test = first | (i << 8) | low[search2 >> 16];
                    if (((test * mult + add) & 0x7fff0000) == second)
                    {
                        origin.Add(test);
                    }
                }
            }
            return origin;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string selectedLetter;
            int selectedNature, selectedLetterTest;


            selectedLetter = FormComboBox.Text;
            selectedLetterTest = characters.IndexOf(selectedLetter);

            if (selectedLetterTest < 0 & FormCheckBox.Checked)
            {
                MessageBox.Show("Error: Chosen Form has not been entered properly, please fix this or uncheck the Form box if you want results.");
                return;
            }

            selectedNature = natures.IndexOf(NatureComboBox.Text);
            if (selectedNature < 0 & NatureCheckBox.Checked)
            {
                MessageBox.Show("Error: Chosen Nature has not been entered properly, please fix this or uncheck the Nature box if you want results.");
                return;
            }

            List<decimal> minIVs = new List<decimal> { MinHPUD.Value, MinATKUD.Value, MinDEFUD.Value, MinSPAUD.Value, MinSPDUD.Value, MinSPEUD.Value };
            List<decimal> maxIVs = new List<decimal> { MaxHPUD.Value, MaxATKUD.Value, MaxDEFUD.Value, MaxSPAUD.Value, MaxSPDUD.Value, MaxSPEUD.Value };

            int j = 0;

            while (j < 6)
            {
                if (maxIVs[j] < minIVs[j] | minIVs[j] > maxIVs[j])
                {
                    MessageBox.Show("Error: IV Range has not been entered properly.");
                    break;
                }
                j += 1;
            }

            dataGridView1.Rows.Clear();
            List<uint> origin = new List<uint>();
            for (uint hp = (uint)MinHPUD.Value; hp <= MaxHPUD.Value; hp++)
            {
                for (uint atk = (uint)MinHPUD.Value; atk <= MaxHPUD.Value; atk++)
                {
                    for (uint def = (uint)MinHPUD.Value; def <= MaxHPUD.Value; def++)
                    {
                        for (uint spe = (uint)MinHPUD.Value; spe <= MaxHPUD.Value; spe++)
                        {
                            for (uint spa = (uint)MinHPUD.Value; spa <= MaxHPUD.Value; spa++)
                            {
                                for (uint spd = (uint)MinHPUD.Value; spd <= MaxHPUD.Value; spd++)
                                {
                                    origin.AddRange(Recover(hp, atk, def, spa, spd, spe));
                                }
                            }
                        }
                    }
                }
            }

            foreach (uint val in origin)
            {
                List<bool> xoredlist = new List<bool> { false, true };
                foreach (bool opp in xoredlist)
                {
                    uint valmod = val;
                    if (opp)
                    {
                        valmod = valmod ^ 0x80000000;
                    }
                    PokeRNGR rng = new PokeRNGR(valmod);
                    uint plow = rng.nextUShort();
                    uint phigh = rng.nextUShort();
                    uint pid = phigh << 16 | plow;
                    bool sflag = false;
                    uint seed;


                    while (!sflag)
                    {
                        PokeRNGR go = new PokeRNGR(rng.seed);
                        uint plowtest = go.nextUShort();
                        uint phightest = go.nextUShort();
                        uint pidtest = phightest << 16 | plowtest;
                        go.nextUInt();
                        uint slot = go.nextUShort() % 100;
                        List<String> letterslots = GetLetterSlots(slot);
                        if (GetLetter(pidtest) == GetLetter(pid))
                        {
                            sflag = true;
                        }
                        if (letterslots.Contains(GetLetter(pid)) & !sflag)
                        {
                            sflag = true;
                            go.nextUInt();
                            seed = go.nextUInt();

                            LCRNG go2 = new LCRNG(seed);
                            go2.nextUInt();
                            uint slot2 = go2.nextUShort() % 100;
                            go2.nextUInt();
                            string letter = "";
                            uint pid2 = 0;
                            string targetLetter = GetTargetLetter(GetLocation(GetLetter(pid)), slot2);
                            while (letter != targetLetter)
                            {
                                uint high2 = go2.nextUShort();
                                uint low2 = go2.nextUShort();
                                pid2 = (high2 << 16) | low2;
                                letter = GetLetter(pid2);
                            }
                            uint psv = ((pid & 0xFFFF) ^ (pid >> 16)) / 8;
                            string shiny = "No";
                            if (tsv == psv)
                            {
                                shiny = "Yes";
                            }

                            int natureint = (int)(pid % 25);
                            string nature = natures[natureint];

                            uint iv1 = go2.nextUShort();
                            uint iv2 = go2.nextUShort();
                            List<uint> ivs = GetIVs(iv1, iv2);
                            //cnt + startingFrame, seed.ToString("X"), pid.ToString("X"), psv, shiny, slot, letter, nature, ivs[0], ivs[1], ivs[2], ivs[3], ivs[4], ivs[5], GetHPowerType(ivs), GetHPowerDamage(ivs)

                            bool flag = true;

                            if (ShinyCheckBox.Checked)
                            {
                                if (!(tsv == psv))
                                {
                                    flag = false;
                                }

                            }


                            if (FormCheckBox.Checked & !(selectedLetter == targetLetter))
                            {
                                flag = false;
                            }

                            if (NatureCheckBox.Checked & !(selectedNature == natureint))
                            {
                                flag = false;
                            }

                            if (flag)
                            {
                                dataGridView1.Rows.Add(seed.ToString("X"), pid2.ToString("X"), psv, shiny, slot, GetLetter(pid2), nature, ivs[0], ivs[1], ivs[2], ivs[3], ivs[4], ivs[5], GetHPowerType(ivs), GetHPowerDamage(ivs));

                            }
                        }

                        rng.nextUInt();
                        rng.nextUInt();

                    }

                }


            }


        }

        public List<string> natures = new List<string> {
                "Hardy","Lonely","Brave","Adamant","Naughty",
                "Bold","Docile","Relaxed","Impish","Lax",
                "Timid","Hasty","Serious","Jolly","Naive",
                "Modest","Mild","Quiet","Bashful","Rash",
                "Calm","Gentle","Sassy","Careful","Quirky"};
        public List<string> hpowertypes = new List<string> { "Fighting", "Flying", "Poison",
                                                             "Ground", "Rock", "Bug",
                                                             "Ghost", "Steel", "Fire",
                                                             "Water", "Grass", "Electric",
                                                             "Psychic", "Ice", "Dragon",
                                                                        "Dark" };

        private string GetHPowerType(List<uint> ivs)
        {
            uint a, b, c, d, e, f;

            a = ivs[0] & 1;
            b = ivs[1] & 1;
            c = ivs[2] & 1;
            d = ivs[5] & 1;
            e = ivs[3] & 1;
            f = ivs[4] & 1;

            return hpowertypes[(int)(((a + 2 * b + 4 * c + 8 * d + 16 * e + 32 * f) * 15) / 63)];
        }

        private uint GetHPowerDamage(List<uint> ivs)
        {
            uint u, v, w, x, y, z;

            u = (ivs[0] >> 1) & 1;
            v = (ivs[1] >> 1) & 1;
            w = (ivs[2] >> 1) & 1;
            x = (ivs[5] >> 1) & 1;
            y = (ivs[3] >> 1) & 1;
            z = (ivs[4] >> 1) & 1;

            return ((u + 2 * v + 4 * w + 8 * x + 16 * y + 32 * z) * 40) / 63 + 30;

        }

        private List<uint> GetIVs(uint iv1, uint iv2)
        {
            uint hp = iv1 & 0x1f;
            uint atk = (iv1 >> 5) & 0x1f;
            uint defense = (iv1 >> 10) & 0x1f;
            uint spa = (iv2 >> 5) & 0x1f;
            uint spd = (iv2 >> 10) & 0x1f;
            uint spe = iv2 & 0x1f;
            return new List<uint> { hp, atk, defense, spa, spd, spe };
        }

        public List<string> characters = new List<string> { "A", "B", "C", "D", "E",
                                                            "F", "G", "H", "I", "J",
                                                            "K", "L", "M", "N", "O",
                                                            "P", "Q", "R", "S", "T",
                                                            "U", "V", "W", "X", "Y",
                                                                 "Z", "!", "?" };
        private String GetLetter(uint pid)
        {
            uint val1, val2, val3, val4, val;
            val1 = (pid >> 24) & 3;
            val2 = (pid >> 16) & 3;
            val3 = (pid >> 8) & 3;
            val4 = pid & 3;
            val = (val1 << 6) | (val2 << 4) | (val3 << 2) | val4;
            return characters[(int)val % 28];
        }

        private String GetTargetLetter(uint location, uint slot)
        {
            if (location == 0)
            {
                if (slot < 99)
                {
                    return "A";
                }
                else
                {
                    return "?";
                }
            }
            if (location == 1)
            {
                if (slot < 50)
                {
                    return "C";
                }
                if (slot < 80)
                {
                    return "D";
                }
                if (slot < 94)
                {
                    return "H";
                }
                if (slot < 99)
                {
                    return "U";
                }
                else
                {
                    return "O";
                }
            }
            if (location == 2)
            {
                if (slot < 60)
                {
                    return "N";
                }
                if (slot < 90)
                {
                    return "S";
                }
                if (slot < 98)
                {
                    return "I";
                }
                else
                {
                    return "E";
                }
            }
            if (location == 3)
            {
                if (slot < 40)
                {
                    return "P";
                }
                if (slot < 60)
                {
                    return "L";
                }
                if (slot < 80)
                {
                    return "J";
                }
                if (slot < 94)
                {
                    return "R";
                }
                else
                {
                    return "Q";
                }
            }
            if (location == 4)
            {
                if (slot < 40)
                {
                    return "Y";
                }
                if (slot < 60)
                {
                    return "T";
                }
                if (slot < 85)
                {
                    return "G";
                }
                if (slot < 98)
                {
                    return "F";
                }
                else
                {
                    return "K";
                }
            }
            if (location == 5)
            {
                if (slot < 50)
                {
                    return "V";
                }
                if (slot < 80)
                {
                    return "W";
                }
                if (slot < 90)
                {
                    return "X";
                }
                if (slot < 98)
                {
                    return "M";
                }
                else
                {
                    return "B";
                }
            }
            if (location == 6)
            {
                if (slot < 99)
                {
                    return "Z";
                }
                else
                {
                    return "!";
                }
            }
            return "0";

        }
        private uint GetLocation(String letter)
        {
            List<string> location0 = new List<string> { "A", "?" };
            List<string> location1 = new List<string> { "C", "D", "H", "U", "O" };
            List<string> location2 = new List<string> { "N", "S", "I", "E" };
            List<string> location3 = new List<string> { "P", "J", "L", "R", "Q" };
            List<string> location4 = new List<string> { "Y", "G", "T", "F", "K" };
            List<string> location5 = new List<string> { "V", "W", "X", "M", "B" };
            List<string> location6 = new List<string> { "Z", "!" };

            if (location0.Contains(letter))
            {
                return 0;
            }
            if (location1.Contains(letter))
            {
                return 1;
            }
            if (location2.Contains(letter))
            {
                return 2;
            }
            if (location3.Contains(letter))
            {
                return 3;
            }
            if (location4.Contains(letter))
            {
                return 4;
            }
            if (location5.Contains(letter))
            {
                return 5;
            }
            if (location6.Contains(letter))
            {
                return 6;
            }
            return 7;

        }

        private List<String> GetLetterSlots(uint slot)
        {
            List<String> letterslots = new List<String>();
            if (slot < 99)
            {
                letterslots.Add("A");
                letterslots.Add("Z");
            }
            else
            {
                letterslots.Add("?");
                letterslots.Add("!");
            }


            if (slot < 50)
            {
                letterslots.Add("C");
            }
            else if (slot < 80)
            {
                letterslots.Add("D");
            }
            else if (slot < 94)
            {
                letterslots.Add("H");
            }
            else if (slot < 99)
            {
                letterslots.Add("U");
            }
            else
            {
                letterslots.Add("O");
            }

            if (slot < 60)
            {
                letterslots.Add("N");
            }
            else if (slot < 90)
            {
                letterslots.Add("S");
            }
            else if (slot < 98)
            {
                letterslots.Add("I");
            }
            else
            {
                letterslots.Add("E");
            }

            if (slot < 40)
            {
                letterslots.Add("P");
            }
            else if (slot < 60)
            {
                letterslots.Add("L");
            }
            else if (slot < 80)
            {
                letterslots.Add("J");
            }
            else if (slot < 94)
            {
                letterslots.Add("R");
            }
            else
            {
                letterslots.Add("Q");
            }

            if (slot < 40)
            {
                letterslots.Add("Y");
            }
            else if (slot < 60)
            {
                letterslots.Add("T");
            }
            else if (slot < 85)
            {
                letterslots.Add("G");
            }
            else if (slot < 98)
            {
                letterslots.Add("F");
            }
            else
            {
                letterslots.Add("K");
            }

            if (slot < 50)
            {
                letterslots.Add("V");
            }
            else if (slot < 80)
            {
                letterslots.Add("W");
            }
            else if (slot < 90)
            {
                letterslots.Add("X");
            }
            else if (slot < 98)
            {
                letterslots.Add("M");
            }
            else
            {
                letterslots.Add("B");
            }



            return letterslots;

        }
        private void AdvancedCheck_CheckedChanged(object sender, EventArgs e)
        {
            dataGridView1.Columns["Seed"].Visible = AdvancedCheck.Checked;
            dataGridView1.Columns["PSV"].Visible = AdvancedCheck.Checked;
            dataGridView1.Columns["Slot"].Visible = AdvancedCheck.Checked;
        }

        private void Searcher_Load(object sender, EventArgs e)
        {

        }
    }
}


