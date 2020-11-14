using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Unown
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void GenerateFrames(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            uint initial,location;
            try
            {
                initial = uint.Parse(SeedTextBox.Text, System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                MessageBox.Show("Error: Seed has not been entered properly, please fix this if you want results.");
                return;
            }
            try
            {
                location = uint.Parse(LocationComboBox.Text.Substring(0, 1));
            }
            catch
            {
                MessageBox.Show("Error: Location has not been entered properly, please fix this if you want results.");
                return;
            }
            LCRNG rng = new LCRNG(initial);
            uint startingFrame, maxFrames;
            try
            {
                startingFrame = uint.Parse(StartingFrameTextBox.Text);
                maxFrames = uint.Parse(FrameAmountTextBox.Text);
            }
            catch
            {
                MessageBox.Show("Error: Starting Frame and/or Frame Amount have not been entered properly, please fix this if you want results.");
                return;
            }
            List<decimal> minIVs = new List<decimal> { MinHPUD.Value, MinATKUD.Value, MinDEFUD.Value, MinSPAUD.Value, MinSPDUD.Value, MinSPEUD.Value };
            List<decimal> maxIVs = new List<decimal> { MaxHPUD.Value, MaxATKUD.Value, MaxDEFUD.Value, MaxSPAUD.Value, MaxSPDUD.Value, MaxSPEUD.Value };

            int delay = 0;

            if (DelayCheckBox.Checked)
            {
                try
                {
                    delay = int.Parse(DelayTextBox.Text);
                }
                catch
                {
                    MessageBox.Show("Error: Delay has not been entered properly, please fix this if you want it to impact the frames or uncheck the Delay box.");
                }
            }

            uint f = 1;

            while (f < startingFrame + delay)
            {
                rng.nextUInt();
                f += 1;
            }
            uint tid, sid, tsv;
            try
            {
                tid = uint.Parse(TIDTextBox.Text);
                sid = uint.Parse(SIDTextBox.Text);
                tsv = (tid ^ sid) / 8;
            }
            catch
            {
                tsv = 8193;
                MessageBox.Show("Error: TID/SID have not been entered properly, please fix this if you want shinies to be marked correctly.");
            }

            string selectedLetter, selectedNature;
            try
            {
                selectedLetter = FormComboBox.Text;
            }
            catch
            {
                MessageBox.Show("Error: Chosen Form has not been entered properly, please fix this or uncheck the Form box if you want results.");
                return;
            }
            try
            {
                selectedNature = NatureCheckBox.Text;
            }
            catch
            {
                MessageBox.Show("Error: Chosen Nature has not been entered properly, please fix this or uncheck the Nature box if you want results.");
                return;
            }


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

            uint cnt = 0;

            while (cnt < maxFrames + delay)
            {
                bool flag = true;
                uint seed = rng.seed;
                LCRNG go = new LCRNG(rng.nextUInt());
                uint slot = go.nextUShort() % 100;
                string targetLetter = GetTargetLetter(location, slot);
                go.nextUInt();
                string letter = "";
                uint pid = 0;
                while (letter != targetLetter)
                {
                    uint high = go.nextUShort();
                    uint low = go.nextUShort();
                    pid = (high << 16) | low;
                    letter = GetLetter(pid);
                }
                uint iv1 = go.nextUShort();
                uint iv2 = go.nextUShort();
                List<uint> ivs = GetIVs(iv1, iv2);
                string shiny = "No";
                
                
                uint psv = ((pid & 0xFFFF) ^ (pid >> 16)) / 8;
                string nature = natures[(int)(pid % 25)];

                if (tsv == psv)
                {
                    shiny = "Yes";
                }
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

                if (NatureCheckBox.Checked & !(selectedNature == nature))
                {
                    flag = false;
                }

                int i = 0;

                while (i<6)
                {
                    if (ivs[i] < minIVs[i] | ivs[i] > maxIVs[i])
                    {
                        flag = false;
                        break;
                    }
                    i += 1;
                }

                if (flag)
                {
                    dataGridView1.Rows.Add(cnt + startingFrame, seed.ToString("X"), pid.ToString("X"), psv, shiny, slot, letter, nature, ivs[0], ivs[1], ivs[2], ivs[3], ivs[4], ivs[5], GetHPowerType(ivs), GetHPowerDamage(ivs));
                }
                cnt += 1;
            }



        }
        private void AdvancedCheck_CheckedChanged(object sender, EventArgs e)
        {
            dataGridView1.Columns["Seed"].Visible = AdvancedCheck.Checked;
            dataGridView1.Columns["PSV"].Visible = AdvancedCheck.Checked;
            dataGridView1.Columns["Slot"].Visible = AdvancedCheck.Checked;
        }


        public List<string> characters = new List<string> { "A", "B", "C", "D", "E",
                                                            "F", "G", "H", "I", "J",
                                                            "K", "L", "M", "N", "O",
                                                            "P", "Q", "R", "S", "T",
                                                            "U", "V", "W", "X", "Y", 
                                                                 "Z", "!", "?" };
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
                if (slot < 40) {
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

        private void PIDToLetterButton_Click(object sender, EventArgs e)
        {
            PIDToLetterForm pidform = new PIDToLetterForm();
            pidform.Show();
        }
    }
}
