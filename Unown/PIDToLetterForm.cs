using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Unown
{
    public partial class PIDToLetterForm : Form
    {
        public PIDToLetterForm()
        {
            InitializeComponent();
        }

        public List<string> characters = new List<string> { "A", "B", "C", "D", "E",
                                                            "F", "G", "H", "I", "J",
                                                            "K", "L", "M", "N", "O",
                                                            "P", "Q", "R", "S", "T",
                                                            "U", "V", "W", "X", "Y",
                                                                 "Z", "EM", "QM" };

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

        private void button1_Click(object sender, EventArgs e)
        {
            uint pid = uint.Parse(PIDTextBox.Text, System.Globalization.NumberStyles.HexNumber);
            string letter = GetLetter(pid);
            ComponentResourceManager resources = new ComponentResourceManager(typeof(PIDToLetterForm));
            pictureBox1.Image = (Image)resources.GetObject("unown-" + letter.ToLower());
        }
    }
}
