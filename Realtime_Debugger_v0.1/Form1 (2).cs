using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.IO.Ports;
using System.Windows;

using RotatePictureBox;
using Utility.ModifyRegistry;

using System.Text.RegularExpressions;
using Symplus.Controls;

using Gif.Components;
using Emgu.CV;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;



namespace Realtime_Debugger_v0._1
{
    public partial class Rrealtime_Debugger : Form
    {
        #region Defenitions
        SerialPort SD_Com;
        ModifyRegistry User_des = new ModifyRegistry();
        GifDecoder GifImage = new GifDecoder();
        Action Image_R;
        Int16 dummy = 0;
        Rrealtime_Debugger NewForm = null;
        bool New_form_ex = false;
        string[] S_tmp=new string[4];
        private Bitmap image = null;
        float xCoordinate = 0.0f;
        float yCoordinate = 0.0f;
        float[] angle = new float[4];
        float _angle = 0.0f;
        int _num = 0;
        byte Try_Num = 5;
        byte[] _data = new byte[10];
        bool _status = false;
        bool _btmp = false;
        byte _inp = 0;
        byte cnt = 0;
        byte[] empty=new byte[8];
        string[] _com = new string[10];
        byte[] param = new byte[5];
        byte[] Tx_tmp = new byte[20];
        bool thread_enable = true;
        byte[] tmp_SA = new byte[4];
        byte header = 0xAA;
        bool[] S_en=new bool[4];
        bool SD_Com_en = false;
        Int16[] tmp_sen = new Int16[3];
        Int16[] res_tSen = new Int16[8];
        int[] Motor_Speed = new int[4];
        int[] _Motor_Speed = new int[4];
        float[] Motor_Time = new float[4];
        int[] Servo_Speed = new int[4];
        int[] Angle_Speed = new int[4];
        int[] Servo_Duartion = new int[4];
        byte[] Motor_direction=new byte[4];
        byte[] splited_Word = new byte[10];
        char[] charArray;
        byte[] _tmp_Splitedstring = new byte[8];
        byte backlight = 1;
        byte[] _Tool_Name=new byte[8];
        byte[] _Sensor_Enable=new byte[8];
        byte[] Send_first = new byte[4];
        int[] _tmp_angle = new int[4];
        #endregion

        public Rrealtime_Debugger()
        {
            
            InitializeComponent();

            txt_lcd1.MaxLength = 8;
            txt_lcd2.MaxLength = 8;

            lbl_ADC1.Visible = false;
            lbl_ADC2.Visible = false;
            lbl_ADC3.Visible = false;
            lbl_ADC4.Visible = false;
            lbl_ADC5.Visible = false;
            lbl_ADC6.Visible = false;
            lbl_ADC7.Visible = false;
            lbl_ADC8.Visible = false;

            #region picturebox init

            pb_BM1.Controls.Add(pb_W1);
            pb_W1.Location = new Point(33, 43);
            pb_W1.BackColor = Color.Transparent;

            pb_BM2.Controls.Add(pb_W2);
            pb_W2.Location = new Point(33, 43);
            pb_W2.BackColor = Color.Transparent;

            pb_BM4.Controls.Add(pb_W4);
            pb_W4.Location = new Point(33, 43);
            pb_W4.BackColor = Color.Transparent;

            pb_BM3.Controls.Add(pb_W3);
            pb_W3.Location = new Point(33, 43);
            pb_W3.BackColor = Color.Transparent;

            pictureBox3.Controls.Add(SHorn1);
            SHorn1.Location = new Point(0, 12);

            pictureBox17.Controls.Add(SHorn2);
            SHorn2.Location = new Point(0, 12);

            pictureBox15.Controls.Add(SHorn4);
            SHorn4.Location = new Point(0, 12);

            pictureBox11.Controls.Add(SHorn3);
            SHorn3.Location = new Point(0, 12);
            
            #endregion 

            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Text = User_des.Read("User_Com");
            toolStripComboBox1.Items.Add("Auto Search");
            toolStripComboBox1.Items.Add(User_des.Read("User_Com") + "");

            image = (Bitmap)Properties.Resources.horn;
        }

        private void Rrealtime_Debugger_Load(object sender, EventArgs e)
        {
            Image_Roate();
        }
        //**********************************************************************************

        #region Form functionalities

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {
            // update comports and available ports
            if (User_des.Read("User_Com") == "")
            {
                toolStripComboBox1.Items.Clear();
                toolStripComboBox1.Items.Add("Auto Search");
                toolStripComboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            }
            else
            {
                toolStripComboBox1.Items.Clear();
                toolStripComboBox1.Items.Add(User_des.Read("User_Com") + "");
                toolStripComboBox1.Items.Add("Auto Search");
                toolStripComboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // catch the nmber of availeble com-ports
            _num = (int)toolStripComboBox1.SelectedIndex;
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            _Start();
        }

        

        //****************************************************************************************

        #region ADC port
        private void com_ADC1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC1.Text == "   ")
            {
                pb_ADC1.Image = null;
                lbl_SADC1.Visible = false;
                lbl_ADC1.Visible = false;
                cmb_LED1.Visible = false;
                _Sensor_Enable[0] = 0;
            }
            else if (com_ADC1.Text == "LED Light")
            {
                pb_ADC1.Image = Properties.Resources.LED2;
                lbl_SADC1.Visible = true;
                lbl_SADC1.Text = "Status";
                lbl_ADC1.Visible = false;
                cmb_LED1.Visible = true;
                _Sensor_Enable[0] = 0;

            }
            else
            {
                if (com_ADC1.Text == "Touch Sensor")
                {
                    pb_ADC1.Image = Properties.Resources.T;
                    lbl_SADC1.Visible = true;
                    lbl_SADC1.Text = "Value";
                    lbl_ADC1.Visible = true;
                    cmb_LED1.Visible = false;
                    _Tool_Name[0] = (byte)0x85;
                }
                if (com_ADC1.Text == "Light Sensor")
                {
                    pb_ADC1.Image = Properties.Resources.LS;
                    lbl_SADC1.Visible = true;
                    lbl_SADC1.Text = "Value";
                    lbl_ADC1.Visible = true;
                    cmb_LED1.Visible = false;
                    _Tool_Name[0] = (byte)0x15;
                }
                if (com_ADC1.Text == "Sound Sensor")
                {
                    pb_ADC1.Image = Properties.Resources.SS;
                    lbl_SADC1.Visible = true;
                    lbl_SADC1.Text = "Value";
                    lbl_ADC1.Visible = true;
                    cmb_LED1.Visible = false;
                    _Tool_Name[0] = (byte)0x25;
                }
                if (com_ADC1.Text == "Infrared Sensor")
                {
                    pb_ADC1.Image = Properties.Resources.IR;
                    lbl_SADC1.Visible = true;
                    lbl_SADC1.Text = "Value";
                    lbl_ADC1.Visible = true;
                    cmb_LED1.Visible = false;
                    _Tool_Name[0] = (byte)0x45;
                }
                _Sensor_Enable[0] = 1;
            }


        }

        private void com_ADC2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC2.Text == "   ")
            {
                pb_ADC2.Image = null;
                lbl_SADC2.Visible = false;
                lbl_ADC2.Visible = false;
                cmb_LED2.Visible = false;
                _Sensor_Enable[1] = 0;
            }
            else if (com_ADC2.Text == "LED Light")
            {
                pb_ADC2.Image = Properties.Resources.LED2;
                lbl_SADC2.Visible = true;
                lbl_SADC2.Text = "Status";
                lbl_ADC2.Visible = false;
                cmb_LED2.Visible = true;
                _Sensor_Enable[1] = 0;

            }
            else
            {
                if (com_ADC2.Text == "Touch Sensor")
                {
                    pb_ADC2.Image = Properties.Resources.T;
                    lbl_SADC2.Visible = true;
                    lbl_SADC2.Text = "Value";
                    lbl_ADC2.Visible = true;
                    cmb_LED2.Visible = false;
                    _Tool_Name[1] = (byte)0x85;
                }
                if (com_ADC2.Text == "Light Sensor")
                {
                    pb_ADC2.Image = Properties.Resources.LS;
                    lbl_SADC2.Visible = true;
                    lbl_SADC2.Text = "Value";
                    lbl_ADC2.Visible = true;
                    cmb_LED2.Visible = false;
                    _Tool_Name[1] = (byte)0x15;
                }
                if (com_ADC2.Text == "Sound Sensor")
                {
                    pb_ADC2.Image = Properties.Resources.SS;
                    lbl_SADC2.Visible = true;
                    lbl_SADC2.Text = "Value";
                    lbl_ADC2.Visible = true;
                    cmb_LED2.Visible = false;
                    _Tool_Name[1] = (byte)0x25;
                }
                if (com_ADC2.Text == "Infrared Sensor")
                {
                    pb_ADC2.Image = Properties.Resources.IR;
                    lbl_SADC2.Visible = true;
                    lbl_SADC2.Text = "Value";
                    lbl_ADC2.Visible = true;
                    cmb_LED2.Visible = false;
                    _Tool_Name[1] = (byte)0x45;
                }
                _Sensor_Enable[1] = 1;
            }
        }

        private void com_ADC4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC4.Text == "   ")
            {
                pb_ADC4.Image = null;
                lbl_SADC4.Visible = false;
                lbl_ADC4.Visible = false;
                cmb_LED4.Visible = false;
                _Sensor_Enable[3] = 0;
            }
            if (com_ADC4.Text == "LED Light")
            {
                pb_ADC4.Image = Properties.Resources.LED2;
                lbl_SADC4.Visible = true;
                lbl_SADC4.Text = "Status";
                lbl_ADC4.Visible = false;
                cmb_LED4.Visible = true;
                _Sensor_Enable[3] = 0;
            }
            else
            {
                if (com_ADC4.Text == "Touch Sensor")
                {
                    pb_ADC4.Image = Properties.Resources.T;
                    lbl_SADC4.Visible = true;
                    lbl_SADC4.Text = "Value";
                    lbl_ADC4.Visible = true;
                    cmb_LED4.Visible = false;
                    _Tool_Name[3] = (byte)0x85;
                }
                if (com_ADC4.Text == "Light Sensor")
                {
                    pb_ADC4.Image = Properties.Resources.LS;
                    lbl_SADC4.Visible = true;
                    lbl_SADC4.Text = "Value";
                    lbl_ADC4.Visible = true;
                    cmb_LED4.Visible = false;
                    _Tool_Name[3] = (byte)0x15;
                }
                if (com_ADC4.Text == "Sound Sensor")
                {
                    pb_ADC4.Image = Properties.Resources.SS;
                    lbl_SADC4.Visible = true;
                    lbl_SADC4.Text = "Value";
                    lbl_ADC4.Visible = true;
                    cmb_LED4.Visible = false;
                    _Tool_Name[3] = (byte)0x25;
                }
                if (com_ADC4.Text == "Infrared Sensor")
                {
                    pb_ADC4.Image = Properties.Resources.IR;
                    lbl_SADC4.Visible = true;
                    lbl_SADC4.Text = "Value";
                    lbl_ADC4.Visible = true;
                    cmb_LED4.Visible = false;
                    _Tool_Name[3] = (byte)0x45;
                }
                _Sensor_Enable[3] = 1;
            }
        }

        private void com_ADC3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC3.Text == "   ")
            {
                pb_ADC3.Image = null;
                lbl_SADC3.Visible = false;
                lbl_ADC3.Visible = false;
                cmb_LED3.Visible = false;
                _Sensor_Enable[2] = 0;
            }
            else if (com_ADC3.Text == "LED Light")
            {
                pb_ADC3.Image = Properties.Resources.LED2;
                lbl_SADC3.Visible = true;
                lbl_SADC3.Text = "Status";
                lbl_ADC3.Visible = false;
                cmb_LED3.Visible = true;
                _Sensor_Enable[2] = 0;
            }
            else
            {
                if (com_ADC3.Text == "Touch Sensor")
                {
                    pb_ADC3.Image = Properties.Resources.T;
                    lbl_SADC3.Visible = true;
                    lbl_SADC3.Text = "Value";
                    lbl_ADC3.Visible = true;
                    cmb_LED3.Visible = false;
                    _Tool_Name[2] = (byte)0x85;
                }
                if (com_ADC3.Text == "Light Sensor")
                {
                    pb_ADC3.Image = Properties.Resources.LS;
                    lbl_SADC3.Visible = true;
                    lbl_SADC3.Text = "Value";
                    lbl_ADC3.Visible = true;
                    cmb_LED3.Visible = false;
                    _Tool_Name[2] = (byte)0x15;
                }
                if (com_ADC3.Text == "Sound Sensor")
                {
                    pb_ADC3.Image = Properties.Resources.SS;
                    lbl_SADC3.Visible = true;
                    lbl_SADC3.Text = "Value";
                    lbl_ADC3.Visible = true;
                    cmb_LED3.Visible = false;
                    _Tool_Name[2] = (byte)0x25;
                }
                if (com_ADC3.Text == "Infrared Sensor")
                {
                    pb_ADC3.Image = Properties.Resources.IR;
                    lbl_SADC3.Visible = true;
                    lbl_SADC3.Text = "Value";
                    lbl_ADC3.Visible = true;
                    cmb_LED3.Visible = false;
                    _Tool_Name[2] = (byte)0x45;
                }
                _Sensor_Enable[2] = 1;
            }
        }

        private void com_ADC5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC5.Text == "   ")
            {
                pb_ADC5.Image = null;
                lbl_SADC5.Visible = false;
                lbl_ADC5.Visible = false;
                cmb_LED5.Visible = false;
                _Sensor_Enable[4] = 0;
            }
            else if (com_ADC5.Text == "LED Light")
            {
                pb_ADC5.Image = Properties.Resources.LED2;
                lbl_SADC5.Visible = true;
                lbl_SADC5.Text = "Status";
                lbl_ADC5.Visible = false;
                cmb_LED5.Visible = true;
                _Sensor_Enable[4] = 0;
            }
            else
            {
                if (com_ADC5.Text == "Touch Sensor")
                {
                    pb_ADC5.Image = Properties.Resources.T;
                    lbl_SADC5.Visible = true;
                    lbl_SADC5.Text = "Value";
                    lbl_ADC5.Visible = true;
                    cmb_LED5.Visible = false;
                    _Tool_Name[4] = (byte)0x85;
                }
                if (com_ADC5.Text == "Light Sensor")
                {
                    pb_ADC5.Image = Properties.Resources.LS;
                    lbl_SADC5.Visible = true;
                    lbl_SADC5.Text = "Value";
                    lbl_ADC5.Visible = true;
                    cmb_LED5.Visible = false;
                    _Tool_Name[4] = (byte)0x15;
                }
                if (com_ADC5.Text == "Sound Sensor")
                {
                    pb_ADC5.Image = Properties.Resources.SS;
                    lbl_SADC5.Visible = true;
                    lbl_SADC5.Text = "Value";
                    lbl_ADC5.Visible = true;
                    cmb_LED5.Visible = false;
                    _Tool_Name[4] = (byte)0x25;
                }
                if (com_ADC5.Text == "Infrared Sensor")
                {
                    pb_ADC5.Image = Properties.Resources.IR;
                    lbl_SADC5.Visible = true;
                    lbl_SADC5.Text = "Value";
                    lbl_ADC5.Visible = true;
                    cmb_LED5.Visible = false;
                    _Tool_Name[4] = (byte)0x45;
                }
                _Sensor_Enable[4] = 1;
            }
        }

        private void com_ADC6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC6.Text == "   ")
            {
                pb_ADC6.Image = null;
                lbl_SADC6.Visible = false;
                lbl_ADC6.Visible = false;
                cmb_LED6.Visible = false;
                _Sensor_Enable[5] = 0;
            }
            else if (com_ADC6.Text == "LED Light")
            {
                pb_ADC6.Image = Properties.Resources.LED2;
                lbl_SADC6.Visible = true;
                lbl_SADC6.Text = "Status";
                lbl_ADC6.Visible = false;
                cmb_LED6.Visible = true;
                _Sensor_Enable[5] = 0;
            }
            else
            {
                if (com_ADC6.Text == "Touch Sensor")
                {
                    pb_ADC6.Image = Properties.Resources.T;
                    lbl_SADC6.Visible = true;
                    lbl_SADC6.Text = "Value";
                    lbl_ADC6.Visible = true;
                    cmb_LED6.Visible = false;
                    _Tool_Name[5] = (byte)0x85;
                }
                if (com_ADC6.Text == "Light Sensor")
                {
                    pb_ADC6.Image = Properties.Resources.LS;
                    lbl_SADC6.Visible = true;
                    lbl_SADC6.Text = "Value";
                    lbl_ADC6.Visible = true;
                    cmb_LED6.Visible = false;
                    _Tool_Name[5] = (byte)0x15;
                }
                if (com_ADC6.Text == "Sound Sensor")
                {
                    pb_ADC6.Image = Properties.Resources.SS;
                    lbl_SADC6.Visible = true;
                    lbl_SADC6.Text = "Value";
                    lbl_ADC6.Visible = true;
                    cmb_LED6.Visible = false;
                    _Tool_Name[5] = (byte)0x25;
                }
                if (com_ADC6.Text == "Infrared Sensor")
                {
                    pb_ADC6.Image = Properties.Resources.IR;
                    lbl_SADC6.Visible = true;
                    lbl_SADC6.Text = "Value";
                    lbl_ADC6.Visible = true;
                    cmb_LED6.Visible = false;
                    _Tool_Name[5] = (byte)0x45;
                }
                _Sensor_Enable[5] = 1;
            }
        }

        private void com_ADC7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC7.Text == "   ")
            {
                pb_ADC7.Image = null;
                lbl_SADC7.Visible = false;
                lbl_ADC7.Visible = false;
                cmb_LED7.Visible = false;
                _Sensor_Enable[6] = 0;
            }
            else if (com_ADC7.Text == "LED Light")
            {
                pb_ADC7.Image = Properties.Resources.LED2;
                lbl_SADC7.Visible = true;
                lbl_SADC7.Text = "Status";
                lbl_ADC7.Visible = false;
                cmb_LED7.Visible = true;
                _Sensor_Enable[6] = 0;
            }
            else
            {
                if (com_ADC7.Text == "Touch Sensor")
                {
                    pb_ADC7.Image = Properties.Resources.T;
                    lbl_SADC7.Visible = true;
                    lbl_SADC7.Text = "Value";
                    lbl_ADC7.Visible = true;
                    cmb_LED7.Visible = false;
                    _Tool_Name[6] = (byte)0x85;
                }
                if (com_ADC7.Text == "Light Sensor")
                {
                    pb_ADC7.Image = Properties.Resources.LS;
                    lbl_SADC7.Visible = true;
                    lbl_SADC7.Text = "Value";
                    lbl_ADC7.Visible = true;
                    cmb_LED7.Visible = false;
                    _Tool_Name[6] = (byte)0x15;
                }
                if (com_ADC7.Text == "Sound Sensor")
                {
                    pb_ADC7.Image = Properties.Resources.SS;
                    lbl_SADC7.Visible = true;
                    lbl_SADC7.Text = "Value";
                    lbl_ADC7.Visible = true;
                    cmb_LED7.Visible = false;
                    _Tool_Name[6] = (byte)0x25;
                }
                if (com_ADC7.Text == "Infrared Sensor")
                {
                    pb_ADC7.Image = Properties.Resources.IR;
                    lbl_SADC7.Visible = true;
                    lbl_SADC7.Text = "Value";
                    lbl_ADC7.Visible = true;
                    cmb_LED7.Visible = false;
                    _Tool_Name[6] = (byte)0x45;
                }
                _Sensor_Enable[6] = 1;
            }
        }

        private void com_ADC8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC8.Text == "   ")
            {
                pb_ADC8.Image = null;
                lbl_SADC8.Visible = false;
                lbl_ADC8.Visible = false;
                cmb_LED8.Visible = false;
                _Sensor_Enable[7] = 0;
            }
            else if (com_ADC8.Text == "LED Light")
            {
                pb_ADC8.Image = Properties.Resources.LED2;
                lbl_SADC8.Visible = true;
                lbl_SADC8.Text = "Status";
                lbl_ADC8.Visible = false;
                cmb_LED8.Visible = true;
                _Sensor_Enable[7] = 0;
            }
            else
            {
                if (com_ADC8.Text == "Touch Sensor")
                {
                    pb_ADC8.Image = Properties.Resources.T;
                    lbl_SADC8.Visible = true;
                    lbl_SADC8.Text = "Value";
                    lbl_ADC8.Visible = true;
                    cmb_LED8.Visible = false;
                    _Tool_Name[7] = (byte)0x85;
                }
                if (com_ADC8.Text == "Light Sensor")
                {
                    pb_ADC8.Image = Properties.Resources.LS;
                    lbl_SADC8.Visible = true;
                    lbl_SADC8.Text = "Value";
                    lbl_ADC8.Visible = true;
                    cmb_LED8.Visible = false;
                    _Tool_Name[7] = (byte)0x15;
                }
                if (com_ADC8.Text == "Sound Sensor")
                {
                    pb_ADC8.Image = Properties.Resources.SS;
                    lbl_SADC8.Visible = true;
                    lbl_SADC8.Text = "Value";
                    lbl_ADC8.Visible = true;
                    cmb_LED8.Visible = false;
                    _Tool_Name[7] = (byte)0x25;
                }
                if (com_ADC8.Text == "Infrared Sensor")
                {
                    pb_ADC8.Image = Properties.Resources.IR;
                    lbl_SADC8.Visible = true;
                    lbl_SADC8.Text = "Value";
                    lbl_ADC8.Visible = true;
                    cmb_LED8.Visible = false;
                    _Tool_Name[7] = (byte)0x45;
                }
                _Sensor_Enable[7] = 1;
            }
        }

        #endregion

        #region DC Motors

        private void trb_SM1_Scroll(object sender, EventArgs e)
        {
            if (Motor_Speed[0] > 0) { Motor_direction[0] = 0; tb_StM1.Text = "CW"; }
            else if (Motor_Speed[0] < 0) { Motor_direction[0] = 1; tb_StM1.Text = "CCW"; }
            else tb_StM1.Text = "Stop";
            lbl_SM1.Text = Math.Abs(Motor_Speed[0]).ToString();
        }

        private void trb_SM2_Scroll(object sender, EventArgs e)
        {
            if (Motor_Speed[1] > 0) { Motor_direction[1] = 0; tb_StM2.Text = "CW"; }
            else if (Motor_Speed[1] < 0) { Motor_direction[1] = 1; tb_StM2.Text = "CCW"; }
            else tb_StM2.Text = "Stop";
            lbl_SM2.Text = Math.Abs(Motor_Speed[1]).ToString();
        }

        private void trb_SM3_Scroll(object sender, EventArgs e)
        {
            if (Motor_Speed[3] > 0) { Motor_direction[3] = 0; tb_StM4.Text = "CW"; }
            else if (Motor_Speed[3] < 0) { Motor_direction[3] = 1; tb_StM4.Text = "CCW"; }
            else tb_StM4.Text = "Stop";
            lbl_SM4.Text = Math.Abs(Motor_Speed[3]).ToString();
        }

        private void trb_SM4_Scroll(object sender, EventArgs e)
        {
            if (Motor_Speed[2] > 0) { Motor_direction[2] = 0; tb_StM3.Text = "CW"; }
            else if (Motor_Speed[2] < 0) { Motor_direction[2] = 1; tb_StM3.Text = "CCW"; }
            else tb_StM3.Text = "Stop";
            lbl_SM3.Text = Math.Abs(Motor_Speed[2]).ToString();
        }

        private void btn_M1_Click(object sender, EventArgs e)
        {
            Motor_Speed[0] = 0;
            trb_SM1.Value = 100;
            Set_param((byte)0x51, (byte)0x01, (byte)(Math.Abs(Motor_Speed[0])), Motor_direction[0], (byte)Motor_Time[0]);
            Send_first[0] = 0;
            TM1.Dispose();
        }

        private void btn_M2_Click(object sender, EventArgs e)
        {
            Motor_Speed[1] = 0;
            trb_SM2.Value = 100;
            Set_param((byte)0x51, (byte)0x02, (byte)(Math.Abs(Motor_Speed[1])), Motor_direction[1], (byte)Motor_Time[1]);
            Send_first[1] = 0;
            TM2.Dispose();
        }

        private void btn_M3_Click(object sender, EventArgs e)
        {
            Motor_Speed[2] = 0;
            trb_SM3.Value = 100;
            Set_param((byte)0x51, (byte)0x03, (byte)(Math.Abs(Motor_Speed[2])), Motor_direction[2], (byte)Motor_Time[2]);
            Send_first[2] = 0;
            TM3.Dispose();
        }

        private void btn_M4_Click(object sender, EventArgs e)
        {
            Motor_Speed[3] = 0;
            trb_SM4.Value = 100;
            Set_param((byte)0x51, (byte)0x04, (byte)(Math.Abs(Motor_Speed[3])), Motor_direction[3], (byte)Motor_Time[3]);
            Send_first[3] = 0;
            TM4.Dispose();
        }

        private void btn_SyncSendM_Click(object sender, EventArgs e)
        {
            if (cmb_SyncDM.SelectedIndex != -1)
            {
                if (num_SyncTM.Value != 0)
                {
                    SyncTimer.Enabled = true;
                    SyncTimer.Interval = (int)(num_SyncTM.Value * 1000);
                }
                byte _tmp = 0;
                if (cmb_SyncDM.Text == "CW") _tmp = 0;
                if (cmb_SyncDM.Text == "CCW") _tmp = 1;
                for (byte cnt = 0; cnt < 4; cnt++)
                {
                    if (cmb_SyncDM.Text == "CW")
                    {
                        Motor_Speed[cnt] = (byte)(num_SyncSM.Value);
                        _Motor_Speed[cnt] = Motor_Speed[cnt];

                        trb_SM1.Value = 100 + (byte)(num_SyncSM.Value);
                        trb_SM2.Value = 100 + (byte)(num_SyncSM.Value);
                        trb_SM3.Value = 100 + (byte)(num_SyncSM.Value);
                        trb_SM4.Value = 100 + (byte)(num_SyncSM.Value);
                    }
                    if (cmb_SyncDM.Text == "CCW")
                    {
                        Motor_Speed[cnt] = -(byte)(num_SyncSM.Value);
                        _Motor_Speed[cnt] = Motor_Speed[cnt];

                        trb_SM1.Value = 100 - (byte)(num_SyncSM.Value);
                        trb_SM2.Value = 100 - (byte)(num_SyncSM.Value);
                        trb_SM3.Value = 100 - (byte)(num_SyncSM.Value);
                        trb_SM4.Value = 100 - (byte)(num_SyncSM.Value);
                    }
                }

                lbl_SM1.Text = Math.Abs(Motor_Speed[0]).ToString();
                lbl_SM2.Text = Math.Abs(Motor_Speed[1]).ToString();
                lbl_SM3.Text = Math.Abs(Motor_Speed[2]).ToString();
                lbl_SM4.Text = Math.Abs(Motor_Speed[3]).ToString();

                Set_param((byte)0x51, 0x05, (byte)(num_SyncSM.Value), _tmp, (byte)(num_SyncTM.Value * 10));
                

                Send_first[0] = 1;
                Send_first[1] = 1;
                Send_first[2] = 1;
                Send_first[3] = 1;
            }
            else
            {
                MessageBox.Show("Please select the direction");
            }
        }

        private void btn_SyncStopM_Click(object sender, EventArgs e)
        {
                Motor_Speed[cnt] = 0;
                Set_param((byte)0x51, 0x05, 0, 0, 0);
                Send_first[0] = 0;
                Send_first[1] = 0;
                Send_first[2] = 0; 
                Send_first[3] = 0;
                SyncTimer.Dispose();
                Motor_Speed[0] = 0;
                trb_SM1.Value = 100;
                Motor_Speed[1] = 0;
                trb_SM2.Value = 100;
                Motor_Speed[2] = 0;
                trb_SM3.Value = 100;
                Motor_Speed[3] = 0;
                trb_SM4.Value = 100;    
        }

        private void SyncTimer_Tick(object sender, EventArgs e)
        {
            for (byte cnt = 0; cnt < 4; cnt++)
            {
                Motor_Speed[cnt] = 0;
                Set_param((byte)0x51, cnt, 0, 0, 0);
                Send_first[cnt] = 0;
            }
            SyncTimer.Enabled = false;
        }

        private void trb_SM1_ValueChanged(object sender, EventArgs e)
        {
            Motor_Speed[0] = (int)(-100 + trb_SM1.Value);
        }

        private void trb_SM2_ValueChanged(object sender, EventArgs e)
        {
            Motor_Speed[1] = (int)(-100 + trb_SM2.Value);
        }

        private void trb_SM3_ValueChanged(object sender, EventArgs e)
        {
            Motor_Speed[2] = (int)(-100 + trb_SM3.Value);
        }

        private void trb_SM4_ValueChanged(object sender, EventArgs e)
        {
            Motor_Speed[3] = (int)(-100 + trb_SM4.Value);
        }

        private void btn_SendM1_Click(object sender, EventArgs e)
        {
            if (num_TimeM1.Value == 0)
            {
                _Motor_Speed[0] = Motor_Speed[0];
                Set_param((byte)0x51, (byte)0x01, (byte)(Math.Abs(_Motor_Speed[0])), Motor_direction[0], (byte)(Motor_Time[0] * 10));
                Send_first[0] = 1;
            }
            else
            {
                TM1.Enabled = true;
                TM1.Interval = (int)(num_TimeM1.Value * 1000);
                _Motor_Speed[0] = Motor_Speed[0];
                Set_param((byte)0x51, (byte)0x01, (byte)(Math.Abs(_Motor_Speed[0])), Motor_direction[0], (byte)(Motor_Time[0] * 10));
                Send_first[0] = 1;
            }
        }

        private void btn_SendM2_Click(object sender, EventArgs e)
        {
            if (num_TimeM2.Value == 0)
            {
                _Motor_Speed[1] = Motor_Speed[1];
                Set_param((byte)0x51, (byte)0x02, (byte)(Math.Abs(_Motor_Speed[1])), Motor_direction[1], (byte)(Motor_Time[1] * 10));
                Send_first[1] = 1;
            }
            else
            {
                TM2.Enabled = true;
                TM2.Interval = (int)(num_TimeM2.Value * 1000);
                _Motor_Speed[1] = Motor_Speed[1];
                Set_param((byte)0x51, (byte)0x02, (byte)(Math.Abs(_Motor_Speed[1])), Motor_direction[1], (byte)(Motor_Time[1] * 10));
                Send_first[1] = 1;
            }
        }

        private void btn_SendM3_Click(object sender, EventArgs e)
        {
            if (num_TimeM3.Value == 0)
            {
                _Motor_Speed[2] = Motor_Speed[2];
                Set_param((byte)0x51, (byte)0x03, (byte)(Math.Abs(_Motor_Speed[2])), Motor_direction[2], (byte)(Motor_Time[2] * 10));
                Send_first[2] = 1;
            }
            else
            {
                TM3.Enabled = true;
                TM3.Interval = (int)(num_TimeM3.Value * 1000);
                _Motor_Speed[2] = Motor_Speed[2];
                Set_param((byte)0x51, (byte)0x03, (byte)(Math.Abs(_Motor_Speed[2])), Motor_direction[2], (byte)(Motor_Time[2] * 10));
                Send_first[2] = 1;
            }
        }

        private void btn_SendM4_Click(object sender, EventArgs e)
        {
            if (num_TimeM4.Value == 0)
            {
                _Motor_Speed[3] = Motor_Speed[3];
                Set_param((byte)0x51, (byte)0x04, (byte)(Math.Abs(_Motor_Speed[3])), Motor_direction[3], (byte)(Motor_Time[3] * 10));
                Send_first[3] = 1;
            }
            else
            {
                TM4.Enabled = true;
                TM4.Interval = (int)(num_TimeM4.Value * 1000);
                _Motor_Speed[3] = Motor_Speed[3];
                Set_param((byte)0x51, (byte)0x04, (byte)(Math.Abs(_Motor_Speed[3])), Motor_direction[3], (byte)(Motor_Time[3] * 10));
                Send_first[3] = 1;
            }
        }

        private void TM1_Tick(object sender, EventArgs e)
        {
            TM1.Enabled = false;
            Motor_Speed[0] = 0;
            trb_SM1.Value = 100;
            Set_param((byte)0x51, (byte)0x01, (byte)(Math.Abs(Motor_Speed[0])), Motor_direction[0], (byte)Motor_Time[0]);
            num_TimeM1.Value = 0;
            Send_first[0] = 0;
        }

        private void TM2_Tick(object sender, EventArgs e)
        {
            TM2.Enabled = false;
            Motor_Speed[1] = 0;
            trb_SM2.Value = 100;
            Set_param((byte)0x51, (byte)0x02, (byte)(Math.Abs(Motor_Speed[1])), Motor_direction[1], (byte)Motor_Time[1]);
            num_TimeM2.Value = 0;
            Send_first[1] = 0;
        }

        private void TM3_Tick(object sender, EventArgs e)
        {
            TM3.Enabled = false;
            Motor_Speed[2] = 0;
            trb_SM3.Value = 100;
            Set_param((byte)0x51, (byte)0x03, (byte)(Math.Abs(Motor_Speed[2])), Motor_direction[2], (byte)Motor_Time[2]);
            num_TimeM3.Value = 0;
            Send_first[2] = 0;
        }

        private void TM4_Tick(object sender, EventArgs e)
        {
            TM4.Enabled = false;
            Motor_Speed[3] = 0;
            trb_SM4.Value = 100;
            Set_param((byte)0x51, (byte)0x04, (byte)(Math.Abs(Motor_Speed[3])), Motor_direction[3], (byte)Motor_Time[3]);
            num_TimeM4.Value = 0;
            Send_first[3] = 0;
        }

        #endregion

        #region Timers

        private void num_TimeM1_ValueChanged(object sender, EventArgs e)
        {
            Motor_Time[0] = (float)num_TimeM1.Value;
        }

        private void num_TimeM2_ValueChanged(object sender, EventArgs e)
        {
            Motor_Time[1] = (float)num_TimeM2.Value;
        }

        private void num_TimeM3_ValueChanged(object sender, EventArgs e)
        {
            Motor_Time[2] = (float)num_TimeM3.Value;
        }

        private void num_TimeM4_ValueChanged(object sender, EventArgs e)
        {
            Motor_Time[3] = (float)num_TimeM4.Value;
        }

        #endregion

        #region LED Conf

        private void btn_AllSensors_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                if (cmb_Allsensors.Text == "Clear")
                {
                    pb_ADC1.Image = null; lbl_SADC1.Visible = false; lbl_ADC1.Visible = false; cmb_LED1.Visible = false; com_ADC1.Text = "Disable";
                    pb_ADC2.Image = null; lbl_SADC2.Visible = false; lbl_ADC2.Visible = false; cmb_LED2.Visible = false; com_ADC2.Text = "Disable";
                    pb_ADC3.Image = null; lbl_SADC3.Visible = false; lbl_ADC3.Visible = false; cmb_LED3.Visible = false; com_ADC3.Text = "Disable";
                    pb_ADC4.Image = null; lbl_SADC4.Visible = false; lbl_ADC4.Visible = false; cmb_LED4.Visible = false; com_ADC4.Text = "Disable";
                    pb_ADC5.Image = null; lbl_SADC5.Visible = false; lbl_ADC5.Visible = false; cmb_LED5.Visible = false; com_ADC5.Text = "Disable";
                    pb_ADC6.Image = null; lbl_SADC6.Visible = false; lbl_ADC6.Visible = false; cmb_LED6.Visible = false; com_ADC6.Text = "Disable";
                    pb_ADC7.Image = null; lbl_SADC7.Visible = false; lbl_ADC7.Visible = false; cmb_LED7.Visible = false; com_ADC7.Text = "Disable";
                    pb_ADC8.Image = null; lbl_SADC8.Visible = false; lbl_ADC8.Visible = false; cmb_LED8.Visible = false; com_ADC8.Text = "Disable";
                    for (int cnt = 0; cnt < 8; cnt++)
                    {
                        _Sensor_Enable[cnt] = 0;
                    }
                }
                else if (cmb_Allsensors.Text == "LED Light")
                {
                    pb_ADC1.Image = Properties.Resources.LED2; lbl_SADC1.Visible = true; lbl_SADC1.Text = "Status"; lbl_ADC1.Visible = false; cmb_LED1.Visible = true; com_ADC1.Text = "LED Light";
                    pb_ADC2.Image = Properties.Resources.LED2; lbl_SADC2.Visible = true; lbl_SADC2.Text = "Status"; lbl_ADC2.Visible = false; cmb_LED2.Visible = true; com_ADC2.Text = "LED Light";
                    pb_ADC3.Image = Properties.Resources.LED2; lbl_SADC3.Visible = true; lbl_SADC3.Text = "Status"; lbl_ADC3.Visible = false; cmb_LED3.Visible = true; com_ADC3.Text = "LED Light";
                    pb_ADC4.Image = Properties.Resources.LED2; lbl_SADC4.Visible = true; lbl_SADC4.Text = "Status"; lbl_ADC4.Visible = false; cmb_LED4.Visible = true; com_ADC4.Text = "LED Light";
                    pb_ADC5.Image = Properties.Resources.LED2; lbl_SADC5.Visible = true; lbl_SADC5.Text = "Status"; lbl_ADC5.Visible = false; cmb_LED5.Visible = true; com_ADC5.Text = "LED Light";
                    pb_ADC6.Image = Properties.Resources.LED2; lbl_SADC6.Visible = true; lbl_SADC6.Text = "Status"; lbl_ADC6.Visible = false; cmb_LED6.Visible = true; com_ADC6.Text = "LED Light";
                    pb_ADC7.Image = Properties.Resources.LED2; lbl_SADC7.Visible = true; lbl_SADC7.Text = "Status"; lbl_ADC7.Visible = false; cmb_LED7.Visible = true; com_ADC7.Text = "LED Light";
                    pb_ADC8.Image = Properties.Resources.LED2; lbl_SADC8.Visible = true; lbl_SADC8.Text = "Status"; lbl_ADC8.Visible = false; cmb_LED8.Visible = true; com_ADC8.Text = "LED Light";
                }
                else
                {
                    if (cmb_Allsensors.Text == "Touch Sensor")
                    {
                        pb_ADC1.Image = Properties.Resources.T; lbl_SADC1.Visible = true; lbl_SADC1.Text = "Value"; lbl_ADC1.Visible = true; cmb_LED1.Visible = false; com_ADC1.Text = "Touch Sensor";
                        pb_ADC2.Image = Properties.Resources.T; lbl_SADC2.Visible = true; lbl_SADC2.Text = "Value"; lbl_ADC2.Visible = true; cmb_LED2.Visible = false; com_ADC2.Text = "Touch Sensor";
                        pb_ADC3.Image = Properties.Resources.T; lbl_SADC3.Visible = true; lbl_SADC3.Text = "Value"; lbl_ADC3.Visible = true; cmb_LED3.Visible = false; com_ADC3.Text = "Touch Sensor";
                        pb_ADC4.Image = Properties.Resources.T; lbl_SADC4.Visible = true; lbl_SADC4.Text = "Value"; lbl_ADC4.Visible = true; cmb_LED4.Visible = false; com_ADC4.Text = "Touch Sensor";
                        pb_ADC5.Image = Properties.Resources.T; lbl_SADC5.Visible = true; lbl_SADC5.Text = "Value"; lbl_ADC5.Visible = true; cmb_LED5.Visible = false; com_ADC5.Text = "Touch Sensor";
                        pb_ADC6.Image = Properties.Resources.T; lbl_SADC6.Visible = true; lbl_SADC6.Text = "Value"; lbl_ADC6.Visible = true; cmb_LED6.Visible = false; com_ADC6.Text = "Touch Sensor";
                        pb_ADC7.Image = Properties.Resources.T; lbl_SADC7.Visible = true; lbl_SADC7.Text = "Value"; lbl_ADC7.Visible = true; cmb_LED7.Visible = false; com_ADC7.Text = "Touch Sensor";
                        pb_ADC8.Image = Properties.Resources.T; lbl_SADC8.Visible = true; lbl_SADC8.Text = "Value"; lbl_ADC8.Visible = true; cmb_LED8.Visible = false; com_ADC8.Text = "Touch Sensor";

                        for (int cnt = 0; cnt < 8; cnt++)
                        {
                            _Tool_Name[cnt] = (byte)0x85;
                        }
                    }
                    if (cmb_Allsensors.Text == "Light Sensor")
                    {
                        pb_ADC1.Image = Properties.Resources.LS; lbl_SADC1.Visible = true; lbl_SADC1.Text = "Value"; lbl_ADC1.Visible = true; cmb_LED1.Visible = false; com_ADC1.Text = "Light Sensor";
                        pb_ADC2.Image = Properties.Resources.LS; lbl_SADC2.Visible = true; lbl_SADC2.Text = "Value"; lbl_ADC2.Visible = true; cmb_LED2.Visible = false; com_ADC2.Text = "Light Sensor";
                        pb_ADC3.Image = Properties.Resources.LS; lbl_SADC3.Visible = true; lbl_SADC3.Text = "Value"; lbl_ADC3.Visible = true; cmb_LED3.Visible = false; com_ADC3.Text = "Light Sensor";
                        pb_ADC4.Image = Properties.Resources.LS; lbl_SADC4.Visible = true; lbl_SADC4.Text = "Value"; lbl_ADC4.Visible = true; cmb_LED4.Visible = false; com_ADC4.Text = "Light Sensor";
                        pb_ADC5.Image = Properties.Resources.LS; lbl_SADC5.Visible = true; lbl_SADC5.Text = "Value"; lbl_ADC5.Visible = true; cmb_LED5.Visible = false; com_ADC5.Text = "Light Sensor";
                        pb_ADC6.Image = Properties.Resources.LS; lbl_SADC6.Visible = true; lbl_SADC6.Text = "Value"; lbl_ADC6.Visible = true; cmb_LED6.Visible = false; com_ADC6.Text = "Light Sensor";
                        pb_ADC7.Image = Properties.Resources.LS; lbl_SADC7.Visible = true; lbl_SADC7.Text = "Value"; lbl_ADC7.Visible = true; cmb_LED7.Visible = false; com_ADC7.Text = "Light Sensor";
                        pb_ADC8.Image = Properties.Resources.LS; lbl_SADC8.Visible = true; lbl_SADC8.Text = "Value"; lbl_ADC8.Visible = true; cmb_LED8.Visible = false; com_ADC8.Text = "Light Sensor";

                        for (int cnt = 0; cnt < 8; cnt++)
                        {
                            _Tool_Name[cnt] = (byte)0x15;
                        }
                    }
                    if (cmb_Allsensors.Text == "Sound Sensor")
                    {
                        pb_ADC1.Image = Properties.Resources.SS; lbl_SADC1.Visible = true; lbl_SADC1.Text = "Value"; lbl_ADC1.Visible = true; cmb_LED1.Visible = false; com_ADC1.Text = "Sound Sensor";
                        pb_ADC2.Image = Properties.Resources.SS; lbl_SADC2.Visible = true; lbl_SADC2.Text = "Value"; lbl_ADC2.Visible = true; cmb_LED2.Visible = false; com_ADC2.Text = "Sound Sensor";
                        pb_ADC3.Image = Properties.Resources.SS; lbl_SADC3.Visible = true; lbl_SADC3.Text = "Value"; lbl_ADC3.Visible = true; cmb_LED3.Visible = false; com_ADC3.Text = "Sound Sensor";
                        pb_ADC4.Image = Properties.Resources.SS; lbl_SADC4.Visible = true; lbl_SADC4.Text = "Value"; lbl_ADC4.Visible = true; cmb_LED4.Visible = false; com_ADC4.Text = "Sound Sensor";
                        pb_ADC5.Image = Properties.Resources.SS; lbl_SADC5.Visible = true; lbl_SADC5.Text = "Value"; lbl_ADC5.Visible = true; cmb_LED5.Visible = false; com_ADC5.Text = "Sound Sensor";
                        pb_ADC6.Image = Properties.Resources.SS; lbl_SADC6.Visible = true; lbl_SADC6.Text = "Value"; lbl_ADC6.Visible = true; cmb_LED6.Visible = false; com_ADC6.Text = "Sound Sensor";
                        pb_ADC7.Image = Properties.Resources.SS; lbl_SADC7.Visible = true; lbl_SADC7.Text = "Value"; lbl_ADC7.Visible = true; cmb_LED7.Visible = false; com_ADC7.Text = "Sound Sensor";
                        pb_ADC8.Image = Properties.Resources.SS; lbl_SADC8.Visible = true; lbl_SADC8.Text = "Value"; lbl_ADC8.Visible = true; cmb_LED8.Visible = false; com_ADC8.Text = "Sound Sensor";

                        for (int cnt = 0; cnt < 8; cnt++)
                        {
                            _Tool_Name[cnt] = (byte)0x25;
                        }
                    }
                    if (cmb_Allsensors.Text == "Infrared Sensor")
                    {
                        pb_ADC1.Image = Properties.Resources.IR; lbl_SADC1.Visible = true; lbl_SADC1.Text = "Value"; lbl_ADC1.Visible = true; cmb_LED1.Visible = false; com_ADC1.Text = "Infrared Sensor";
                        pb_ADC2.Image = Properties.Resources.IR; lbl_SADC2.Visible = true; lbl_SADC2.Text = "Value"; lbl_ADC2.Visible = true; cmb_LED2.Visible = false; com_ADC2.Text = "Infrared Sensor";
                        pb_ADC3.Image = Properties.Resources.IR; lbl_SADC3.Visible = true; lbl_SADC3.Text = "Value"; lbl_ADC3.Visible = true; cmb_LED3.Visible = false; com_ADC3.Text = "Infrared Sensor";
                        pb_ADC4.Image = Properties.Resources.IR; lbl_SADC4.Visible = true; lbl_SADC4.Text = "Value"; lbl_ADC4.Visible = true; cmb_LED4.Visible = false; com_ADC4.Text = "Infrared Sensor";
                        pb_ADC5.Image = Properties.Resources.IR; lbl_SADC5.Visible = true; lbl_SADC5.Text = "Value"; lbl_ADC5.Visible = true; cmb_LED5.Visible = false; com_ADC5.Text = "Infrared Sensor";
                        pb_ADC6.Image = Properties.Resources.IR; lbl_SADC6.Visible = true; lbl_SADC6.Text = "Value"; lbl_ADC6.Visible = true; cmb_LED6.Visible = false; com_ADC6.Text = "Infrared Sensor";
                        pb_ADC7.Image = Properties.Resources.IR; lbl_SADC7.Visible = true; lbl_SADC7.Text = "Value"; lbl_ADC7.Visible = true; cmb_LED7.Visible = false; com_ADC7.Text = "Infrared Sensor";
                        pb_ADC8.Image = Properties.Resources.IR; lbl_SADC8.Visible = true; lbl_SADC8.Text = "Value"; lbl_ADC8.Visible = true; cmb_LED8.Visible = false; com_ADC8.Text = "Infrared Sensor";

                        for (int cnt = 0; cnt < 8; cnt++)
                        {
                            _Tool_Name[cnt] = (byte)0x45;
                        }
                    }
                    for (int cnt = 0; cnt < 8; cnt++)
                    {
                        _Sensor_Enable[cnt] = 1;
                    }
                }
            }
        }

        private void cmb_LED1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED1.Text == "ON") Set_param((byte)0x54, (byte)0x01, 1, 0, 0);
            if (cmb_LED1.Text == "OFF") Set_param((byte)0x54, (byte)0x01, 0, 0, 0);
        }

        private void cmb_LED2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED2.Text == "ON") Set_param((byte)0x54, (byte)0x02, 1, 0, 0);
            if (cmb_LED2.Text == "OFF") Set_param((byte)0x54, (byte)0x02, 0, 0, 0);
        }

        private void cmb_LED3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED3.Text == "ON") Set_param((byte)0x54, (byte)0x03, 1, 0, 0);
            if (cmb_LED3.Text == "OFF") Set_param((byte)0x54, (byte)0x03, 0, 0, 0);
        }

        private void cmb_LED4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED4.Text == "ON") Set_param((byte)0x54, (byte)0x04, 1, 0, 0);
            if (cmb_LED4.Text == "OFF") Set_param((byte)0x54, (byte)0x04, 0, 0, 0);
        }

        private void cmb_LED5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED5.Text == "ON") Set_param((byte)0x54, (byte)0x05, 1, 0, 0);
            if (cmb_LED5.Text == "OFF") Set_param((byte)0x54, (byte)0x05, 0, 0, 0);
        }

        private void cmb_LED6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED6.Text == "ON") Set_param((byte)0x54, (byte)0x06, 1, 0, 0);
            if (cmb_LED6.Text == "OFF") Set_param((byte)0x54, (byte)0x06, 0, 0, 0);
        }

        private void cmb_LED7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED7.Text == "ON") Set_param((byte)0x54, (byte)0x07, 1, 0, 0);
            if (cmb_LED7.Text == "OFF") Set_param((byte)0x54, (byte)0x07, 0, 0, 0);
        }

        private void cmb_LED8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED8.Text == "ON") Set_param((byte)0x54, (byte)0x08, 1, 0, 0);
            if (cmb_LED8.Text == "OFF") Set_param((byte)0x54, (byte)0x08, 0, 0, 0);
        }

#endregion

        #region LCD
        private void txt_lcd1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txt_lcd2.Select();
            }
            if ((e.KeyChar == (char)Keys.Back) || (e.KeyChar == (char)Keys.Delete))
            {
                if(txt_lcd1.Text=="")
                {
                    Update_LCD((byte)0x58, backlight, 0, 0, empty);
                }
            }
        }

        private void txt_lcd2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == (char)Keys.Back) || (e.KeyChar == (char)Keys.Delete))
            {
                if (txt_lcd1.Text == "")
                {
                    Update_LCD((byte)0x58, backlight, 0, 1, empty);
                }
            }
        }

        private void chb_Backlight_CheckedChanged(object sender, EventArgs e)
        {
            if (chb_Backlight.Checked)
            {
                backlight = 1;
                txt_lcd1.BackColor = Color.LimeGreen;
                txt_lcd2.BackColor = Color.LimeGreen;
                Update_LCD(0x58, 0x01, 0x00, 0x00, empty);
            }
            else
            {
                backlight = 0;
                txt_lcd1.BackColor = Color.DarkOliveGreen;
                txt_lcd2.BackColor = Color.DarkOliveGreen;
                Update_LCD(0x58, 0x00, 0x00, 0x00, empty);
            }

        }

        private void txt_lcd1_TextChanged(object sender, EventArgs e)
        {
            if (txt_lcd1.Text != null)
            {
                for (byte i = 0; i < 8; i++)
                {
                    _tmp_Splitedstring[i] = 0;
                }

                SplitString(txt_lcd1.Text);

                for (cnt = 0; cnt < charArray.Length; cnt++)
                {
                    _tmp_Splitedstring[cnt] = Convert.ToByte(charArray[cnt]);
                }
            }
            Update_LCD((byte)0x58, backlight, 0, 0, _tmp_Splitedstring);
        }

        private void txt_lcd2_TextChanged(object sender, EventArgs e)
        {
            if (txt_lcd2.Text != null)
            {
                for (byte i = 0; i < 8; i++)
                {
                    _tmp_Splitedstring[i] = 0;
                }

                SplitString(txt_lcd2.Text);

                for (cnt = 0; cnt < charArray.Length; cnt++)
                {
                    _tmp_Splitedstring[cnt] = Convert.ToByte(charArray[cnt]);
                }
            }
            Update_LCD((byte)0x58, backlight, 0, 1, _tmp_Splitedstring);
        }

        #endregion

        #region Servo 

        private void num_S1Angle_ValueChanged(object sender, EventArgs e)
        {
            if (SD_Com_en)
                if (S_en[0])
                {
                    RotateImage(SHorn1, image, (byte)num_S1Angle.Value - 90);
                    Set_param(0x52, 0x01, (byte)num_S1Angle.Value, (byte)tb_SSpeed1.Value, (byte)(num_S1S.Value * 10));
                }
        }

        private void num_S2Angle_ValueChanged(object sender, EventArgs e)
        {
            if (SD_Com_en)
                if (S_en[1])
                {
                    RotateImage(SHorn2, image, (byte)num_S2Angle.Value - 90);
                    Set_param(0x52, 0x02, (byte)num_S2Angle.Value, (byte)tb_SSpeed2.Value, (byte)(num_S2S.Value * 10));
                }
        }

        private void num_S3Angle_ValueChanged(object sender, EventArgs e)
        {
            if(SD_Com_en)
                if (S_en[2])
                {
                    RotateImage(SHorn3, image, (byte)num_S3Angle.Value - 90);
                    Set_param(0x52, 0x03, (byte)num_S3Angle.Value, (byte)tb_SSpeed3.Value, (byte)(num_S3S.Value * 10));
                }
        }

        private void num_S4Angle_ValueChanged(object sender, EventArgs e)
        {
            if (SD_Com_en)
                if (S_en[3])
                {
                    RotateImage(SHorn4, image, (byte)num_S4Angle.Value - 90);
                    Set_param(0x52, 0x04, (byte)num_S4Angle.Value, (byte)tb_SSpeed4.Value, (byte)(num_S4S.Value * 10));
                }
        }

        private void btn_S1en_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                if (btn_S1en.Text == "ON")
                {
                    S_en[0] = false;
                    btn_S1en.Text = "OFF";
                    btn_S1en.BackColor = Color.OrangeRed;
                    Set_param(0x52, 0x01, (byte)0xc8, (byte)tb_SSpeed1.Value, (byte)(num_S1S.Value * 10));
                }
                else
                {
                    S_en[0] = true;
                    btn_S1en.Text = "ON";
                    btn_S1en.BackColor = Color.Green;
                    Set_param(0x52, 0x01, (byte)tmp_SA[0], (byte)tb_SSpeed1.Value, (byte)(num_S1S.Value * 10));
                }
            }
        }

        private void btn_S2en_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                if (btn_S2en.Text == "ON")
                {
                    S_en[1] = false;
                    btn_S2en.Text = "OFF";
                    btn_S2en.BackColor = Color.OrangeRed;
                    Set_param(0x52, 0x02, (byte)0xc8, (byte)tb_SSpeed2.Value, (byte)(num_S2S.Value * 10));
                }
                else
                {
                    S_en[1] = true;
                    btn_S2en.Text = "ON";
                    btn_S2en.BackColor = Color.Green;
                    Set_param(0x52, 0x02, (byte)tmp_SA[1], (byte)tb_SSpeed2.Value, (byte)(num_S2S.Value * 10));
                }
            }
        }

        private void btn_S3en_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                if (btn_S3en.Text == "ON")
                {
                    S_en[2] = false;
                    btn_S3en.Text = "OFF";
                    btn_S3en.BackColor = Color.OrangeRed;
                    Set_param(0x52, 0x03, (byte)0xc8, (byte)tb_SSpeed3.Value, (byte)(num_S3S.Value * 10));
                }
                else
                {
                    S_en[2] = true;
                    btn_S3en.Text = "ON";
                    btn_S3en.BackColor = Color.Green;
                    Set_param(0x52, 0x03, (byte)tmp_SA[2], (byte)tb_SSpeed3.Value, (byte)(num_S3S.Value * 10));
                }
            }
        }

        private void btn_S4en_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                if (btn_S4en.Text == "ON")
                {
                    S_en[3] = false;
                    btn_S4en.Text = "OFF";
                    btn_S4en.BackColor = Color.OrangeRed;
                    Set_param(0x52, 0x04, (byte)0xc8, (byte)tb_SSpeed4.Value, (byte)(num_S4S.Value * 10));
                }
                else
                {
                    S_en[3] = true;
                    btn_S4en.Text = "ON";
                    btn_S4en.BackColor = Color.Green;
                    Set_param(0x52, 0x04, (byte)tmp_SA[3], (byte)tb_SSpeed4.Value, (byte)(num_S4S.Value * 10));
                }
            }
        }

        private void tb_SSpeed1_MouseUp(object sender, MouseEventArgs e)
        {
            Set_param(0x52, 0x01, (byte)tmp_SA[0], (byte)tb_SSpeed1.Value, (byte)(num_S1S.Value * 10));
        }

        private void tb_SSpeed2_MouseUp(object sender, MouseEventArgs e)
        {
            Set_param(0x52, 0x02, (byte)tmp_SA[1], (byte)tb_SSpeed2.Value, (byte)(num_S2S.Value * 10));
        }

        private void tb_SSpeed3_MouseUp(object sender, MouseEventArgs e)
        {
            Set_param(0x52, 0x03, (byte)tmp_SA[2], (byte)tb_SSpeed3.Value, (byte)(num_S3S.Value * 10));
        }

        private void tb_SSpeed4_MouseUp(object sender, MouseEventArgs e)
        {
            Set_param(0x52, 0x04, (byte)tmp_SA[3], (byte)tb_SSpeed4.Value, (byte)(num_S4S.Value * 10));
        }

        private void SHorn1_MouseUp(object sender, MouseEventArgs e)
        {
            if (S_en[0])
            {
                float _tmp = 0;
                if (e != null)
                {
                    xCoordinate = e.X;
                    yCoordinate = e.Y;
                    PointF MPU = new PointF(-(xCoordinate - 45), -(yCoordinate - 39));
                    _tmp = (int)Polar_Cordinate_Extender(MPU);
                }
                else
                {
                    _tmp = (float)Convert.ToInt16(num_S1Angle.Text);
                }

                if (_tmp < 0) _tmp += 360;
                if ((Math.Abs(_tmp) > 180) && (Math.Abs(_tmp) <= 270)) _tmp = 180;
                if ((Math.Abs(_tmp) >= 270)) _tmp = 0;
                num_S1Angle.Text = ((int)_tmp).ToString();
                RotateImage(SHorn1, image, _tmp - 90);
                tmp_SA[0] = (byte)_tmp;
                Set_param(0x52, 0x01, (byte)_tmp, (byte)tb_SSpeed1.Value, (byte)(num_S1S.Value * 10));
            }
            else
            {
                MessageBox.Show("Please turn ON the mentioned servo");
            }
        }

        private void SHorn3_MouseUp(object sender, MouseEventArgs e)
        {
            if (S_en[2])
            {
                float _tmp = 0;
                if (e != null)
                {
                    xCoordinate = e.X;
                    yCoordinate = e.Y;
                    PointF MPU = new PointF(-(xCoordinate - 45), -(yCoordinate - 39));
                    _tmp = (int)Polar_Cordinate_Extender(MPU);
                }
                else
                {
                    _tmp = (float)Convert.ToInt16(num_S3Angle.Text);
                }
                if (_tmp < 0) _tmp += 360;
                if ((Math.Abs(_tmp) > 180) && (Math.Abs(_tmp) <= 270)) _tmp = 180;
                if ((Math.Abs(_tmp) >= 270)) _tmp = 0;
                num_S3Angle.Text = ((int)_tmp).ToString();
                RotateImage(SHorn3, image, _tmp - 90);
                tmp_SA[2] = (byte)_tmp;
                Set_param(0x52, 0x03, (byte)_tmp, (byte)tb_SSpeed3.Value, (byte)(num_S3S.Value * 10));
            }
            else
            {
                MessageBox.Show("Please turn ON the mentioned servo");
            }
        }

        private void SHorn2_MouseUp(object sender, MouseEventArgs e)
        {
            if (S_en[1])
            {
                float _tmp = 0;
                if (e != null)
                {
                    xCoordinate = e.X;
                    yCoordinate = e.Y;
                    PointF MPU = new PointF(-(xCoordinate - 45), -(yCoordinate - 39));
                    _tmp = (int)Polar_Cordinate_Extender(MPU);
                }
                else
                {
                    _tmp = (float)Convert.ToInt16(num_S2Angle.Text);
                }
                if (_tmp < 0) _tmp += 360;
                if ((Math.Abs(_tmp) > 180) && (Math.Abs(_tmp) <= 270)) _tmp = 180;
                if ((Math.Abs(_tmp) >= 270)) _tmp = 0;
                num_S2Angle.Text = ((int)_tmp).ToString();
                RotateImage(SHorn2, image, _tmp - 90);
                tmp_SA[1] = (byte)_tmp;
                Set_param(0x52, 0x02, (byte)_tmp, (byte)tb_SSpeed2.Value, (byte)(num_S2S.Value * 10));
            }
            else
            {
                MessageBox.Show("Please turn ON the mentioned servo");
            }
        }

        private void SHorn4_MouseUp(object sender, MouseEventArgs e)
        {
            if (S_en[3])
            {
                float _tmp = 0;
                if (e != null)
                {
                    xCoordinate = e.X;
                    yCoordinate = e.Y;
                    PointF MPU = new PointF(-(xCoordinate - 45), -(yCoordinate - 39));
                    _tmp = (int)Polar_Cordinate_Extender(MPU);
                }
                else
                {
                    _tmp = (float)Convert.ToInt16(num_S4Angle.Text);
                }
                if (_tmp < 0) _tmp += 360;
                if ((Math.Abs(_tmp) > 180) && (Math.Abs(_tmp) <= 270)) _tmp = 180;
                if ((Math.Abs(_tmp) >= 270)) _tmp = 0;
                num_S4Angle.Text = ((int)_tmp).ToString();
                RotateImage(SHorn4, image, _tmp - 90);
                tmp_SA[3] = (byte)_tmp;
                Set_param(0x52, 0x04, (byte)_tmp, (byte)tb_SSpeed4.Value, (byte)(num_S4S.Value * 10));
            }
            else
            {
                MessageBox.Show("Please turn ON the mentioned servo");
            }
        }

        private void btn_SyncSendS_Click(object sender, EventArgs e)
        {
            int _tmp = 0;
            _tmp = (int)num_SyncAS.Value;

            tmp_SA[0] = (byte)num_SyncAS.Value;
            tmp_SA[1] = (byte)num_SyncAS.Value;
            tmp_SA[2] = (byte)num_SyncAS.Value;
            tmp_SA[3] = (byte)num_SyncAS.Value;

            if (S_en[0])
            {
                RotateImage(SHorn1, image, (int)num_SyncAS.Value - 90);
                num_S1Angle.Text = ((int)_tmp).ToString();
            }
            if (S_en[1])
            {
                RotateImage(SHorn2, image, (int)num_SyncAS.Value - 90);
                num_S2Angle.Text = ((int)_tmp).ToString();
            }
            if (S_en[2])
            {
                RotateImage(SHorn3, image, (int)num_SyncAS.Value - 90);
                num_S3Angle.Text = ((int)_tmp).ToString();
            }
            if (S_en[3])
            {
                RotateImage(SHorn4, image, (int)num_SyncAS.Value - 90);
                num_S4Angle.Text = ((int)_tmp).ToString();
            }

            if (S_en[0] || S_en[1] || S_en[2] || S_en[3])
            Set_param(0x52, 0x05, (byte)num_SyncAS.Value, (byte)num_SyncSS.Value, (byte)(num_SyncDS.Value * 10));
        }

        private void btn_SuncStopS_Click(object sender, EventArgs e)
        {
            if (S_en[0])
            {
                RotateImage(SHorn1, image, -90);
                num_S1Angle.Text = "0";
            }
            if (S_en[1])
            {
                RotateImage(SHorn2, image, -90);
                num_S2Angle.Text = "0";
            }
            if (S_en[2])
            {
                RotateImage(SHorn3, image, -90);
                num_S3Angle.Text = "0";
            }
            if (S_en[3])
            {
                RotateImage(SHorn4, image, -90);
                num_S4Angle.Text = "0";
            }

            if (S_en[0] || S_en[1] || S_en[2] || S_en[3])
            Set_param(0x52, 0x05, 0x00, 0xF0, 0xf0);

            tmp_SA[0] = (byte)num_SyncAS.Value;
            tmp_SA[1] = (byte)num_SyncAS.Value;
            tmp_SA[2] = (byte)num_SyncAS.Value;
            tmp_SA[3] = (byte)num_SyncAS.Value;
        }

        private void tb_SSpeed1_Scroll(object sender, EventArgs e)
        {
            lbl_SS1.Text = (tb_SSpeed1.Value).ToString();
        }

        private void tb_SSpeed2_Scroll(object sender, EventArgs e)
        {
            lbl_SS2.Text = (tb_SSpeed2.Value).ToString();
        }

        private void tb_SSpeed3_Scroll(object sender, EventArgs e)
        {
            lbl_SS3.Text = (tb_SSpeed3.Value).ToString();
        }

        private void tb_SSpeed4_Scroll(object sender, EventArgs e)
        {
            lbl_SS4.Text = (tb_SSpeed4.Value).ToString();
        }

        private void tb_S1Angle_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SHorn1_MouseUp(null, null);
            }
        }

        private void tb_S2Angle_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SHorn2_MouseUp(null, null);
            }
        }

        private void tb_S3Angle_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SHorn3_MouseUp(null, null);
            }
        }

        private void tb_S4Angle_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SHorn4_MouseUp(null, null);
            }
        }

        #endregion

        #endregion

        //***********************************************************************************

        #region Funtions

        /// <summary>
        /// Rotate image 
        /// </summary>
        /// <param name="pb">Proposed PictureBox</param>
        /// <param name="img">Proposed Image that you want to routate it</param>
        /// <param name="angle">Desire angle to routation</param>
        private void RotateImage(PictureBox pb, Image img, float angle)
        {
            if (img == null || pb.Image == null)
                return;

            Image oldImage = pb.Image;
            pb.Image = Utilities.RotateImage(img, angle);
            if (oldImage != null)
            {
                oldImage.Dispose();
            }
        }
        //***********************************************************************************
        /// <summary>
        /// At the first of connection with Auto-search selsection .. this function ping the PRC without the com selection
        /// </summary>
        /// <returns>this function if the Ping progress was successful returns true otherwise false</returns>
        private bool Ping()
        {
            SD_Com.DtrEnable = true;
            try
            {
                _inp = (byte)SD_Com.ReadByte();
                if (_inp == 57)
                {
                    _status = true;
                    _data[0] =_inp;
                    SD_Com.Write(_data, 0, 1);
                    Thread.Sleep(50);
                    _data[0] = 0x55;
                    SD_Com.Write(_data, 0, 1);
                }
            }
            catch
            {
                MessageBox.Show("Connect PRC to PC !!");
            }
            return _status;
        }
        //***********************************************************************************
        /// <summary>
        /// this function handles the all situation about com-port and primery connection
        /// </summary>
        private void _Start()
        {
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("Auto Search");
            toolStripComboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            //###################################################################

            #region Port OPEN
            if (btn_Connect.ToolTipText == "Open")
            {
                #region Autosearch
                if (toolStripComboBox1.Text == "Auto Search")
                {
                    _com = System.IO.Ports.SerialPort.GetPortNames();
                    if (_com.Count() == 0)
                    {
                        MessageBox.Show("Connect PRC to PC !!","Error");
                    }
                    else
                    {
                        try
                        {
                            for (int cnt = 0; cnt < toolStripComboBox1.Items.Count; cnt++)
                            {
                                SD_Com = new SerialPort(_com[cnt], 115200);
                                Thread.Sleep(1);
                                if (SD_Com.IsOpen == false)
                                {
                                    for (byte pd = 0; pd < Try_Num; pd++)
                                    {
                                        Thread.Sleep(100);
                                        try
                                        {
                                            SD_Com.ReadTimeout = 100;
                                            SD_Com.Open();
                                            _btmp = Ping();
                                            Thread.Sleep(1);
                                            if (!_btmp)
                                            {
                                                SD_Com.Close();
                                            }
                                            else if (_btmp)
                                            {
                                                btn_Connect.ToolTipText = "Close";
                                                btn_Connect.Image = Properties.Resources.Disconnect;
                                                toolStripComboBox1.Enabled = false;
                                                SD_Com_en = true;
                                                Read_Thread();
                                                toolStripComboBox1.Text = SD_Com.PortName.ToString();
                                                User_des.Write("User_Com", SD_Com.PortName.ToString());
                                                Console.Beep();
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show(ex.Message.ToString(),"Error");
                                            Console.Beep();
                                            break;
                                        }
                                    }
                                }
                                Thread.Sleep(1);
                                if (_btmp) break;
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Check Connection!","Error");
                            toolStripComboBox1.Text = "Auto Search";
                            Console.Beep();
                            _status = false;
                            _btmp = false;
                            SD_Com.Close();
                            SD_Com = null;
                            btn_Connect.ToolTipText = "Open";
                            btn_Connect.Image = Properties.Resources.Connect;
                            toolStripComboBox1.Enabled = true;
                        }
                    }
                }
                #endregion

                #region Manual
                else if (toolStripComboBox1.Text != "")
                {
                    if (_num == -1)
                    {
                        MessageBox.Show( "Select Com-port!!","Error");
                    }
                    else
                    {
                        SD_Com = new SerialPort(toolStripComboBox1.Text,115200);
                        if (SD_Com.IsOpen == false)
                        {
                            try
                            {
                                toolStripComboBox1.Text = SD_Com.PortName.ToString();
                                SD_Com.ReadTimeout = 100;
                                SD_Com.Open();
                                Ping();
                                User_des.Write("User_Com", SD_Com.PortName.ToString());
                                btn_Connect.ToolTipText = "Close";
                                btn_Connect.Image = Properties.Resources.Disconnect;
                                toolStripComboBox1.Enabled = false;
                                SD_Com_en = true;
                                Read_Thread();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message.ToString(),"Error");
                                toolStripComboBox1.Text = "Auto Search";
                                User_des.Write("User_Com", "");
                                Console.Beep();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Select Port Name","Error");
                    Console.Beep();
                }
                #endregion
            }
            #endregion

            //###################################################################

            #region PORT Close
            else
            {

                Send_close_command();

                Image_R.Clone();
                Thread.Sleep(50);

                btn_Connect.ToolTipText = "Open";
                btn_Connect.Image = Properties.Resources.Connect;
                toolStripComboBox1.Enabled = true;
                SD_Com.Close();

                thread_enable = false;

                NewForm = new Rrealtime_Debugger();
                NewForm.Show();
                New_form_ex = true;
                this.Dispose(false);
            }
            #endregion

        }
        //***********************************************************************************
        /// <summary>
        /// this function calls when ever application is going to close to exit the PRC from RTD mode
        /// </summary>
        private void Send_close_command()
        {
            if (SD_Com_en)
            {
                Tx_tmp[0] = header;
                Tx_tmp[1] = header;
                Tx_tmp[2] = 0x45;
                Tx_tmp[3] = 0x58;

                int checksum = 0x45 + 0x58;
                Tx_tmp[4] = (byte)checksum;
                Tx_tmp[5] = (byte)(checksum >> 8);

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        SD_Com.Write(Tx_tmp, 0, 6);
                    }
                    catch
                    {
                        MessageBox.Show("Problem in Exit");
                    }
                }
                SD_Com.DiscardOutBuffer();
                SD_Com.DiscardInBuffer();
            }
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tool_Name">One of defined components to connet to PRC</param>
        /// <param name="Tool_ID">The port number proposed to connect the sensor or I/O module</param>
        /// <param name="_param0">special parameter considering the type of connected module</param>
        /// <param name="_param1">special parameter considering the type of connected module</param>
        /// <param name="_param2">special parameter considering the type of connected module</param>
        private void Set_param(byte Tool_Name, byte Tool_ID, byte _param0, byte _param1, byte _param2)
        {
            if (SD_Com_en)
            {
                Tx_tmp[0] = header;
                Tx_tmp[1] = header;
                Tx_tmp[2] = Tool_Name;
                Tx_tmp[3] = Tool_ID;
                Tx_tmp[4] = _param0;   //Speed or State
                Tx_tmp[5] = _param1;   //Direction
                Tx_tmp[6] = _param2;   //Time

                int checksum = Tool_Name + Tool_ID + _param0 + _param1 + _param2;
                Tx_tmp[7] = (byte)checksum;
                Tx_tmp[8] = (byte)(checksum >> 8);
                //
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        SD_Com.Write(Tx_tmp, 0, 9);
                    }
                    catch
                    {
                        MessageBox.Show("Problem in Set param");
                    }
                }
                SD_Com.DiscardOutBuffer();
                SD_Com.DiscardInBuffer();
            }
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Backlight">0x01->Backlight of LCD->ON, 0x00->Backlight of LCD->OFF</param>
        /// <param name="x_pos">Desire X position to Write the characters on LCD</param>
        /// <param name="y_pos">Desire y position to Write the characters on LCD</param>
        /// <param name="character">The splitted String to write and show in LCD in byte 'ASCI' with maximmum lenght=8</param>
        private void Update_LCD(byte Tool_Name, byte Backlight, byte x_pos, byte y_pos, byte[] character)
        {
            if (SD_Com_en)
            {
                Tx_tmp[0] = header;
                Tx_tmp[1] = header;
                Tx_tmp[2] = Tool_Name;
                //Backlight control command
                Tx_tmp[3] = Backlight;
                //X-position in LCD 
                Tx_tmp[4] = x_pos;
                //Y-position in LCD 
                Tx_tmp[5] = y_pos;
                //Character of onw line
                Tx_tmp[6] = character[0];
                Tx_tmp[7] = character[1];
                Tx_tmp[8] = character[2];
                Tx_tmp[9] = character[3];
                Tx_tmp[10] = character[4];
                Tx_tmp[11] = character[5];
                Tx_tmp[12] = character[6];
                Tx_tmp[13] = character[7];

                int checksum = 0;
                for (byte cnt = 0; cnt <= 7; cnt++) checksum += character[cnt];
                checksum += Tool_Name;
                checksum += x_pos;
                checksum += y_pos;
                checksum += Backlight;

                Tx_tmp[14] = (byte)checksum;
                Tx_tmp[15] = (byte)(checksum >> 8);

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        SD_Com.Write(Tx_tmp, 0, 16);
                    }
                    catch
                    {
                        MessageBox.Show("Problem in Set LCD param");
                    }
                }
                SD_Com.DiscardOutBuffer();
                SD_Com.DiscardInBuffer();
            }
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tool_Name">One of defined components to connet to PRC</param>
        /// <param name="Tool_ID">The port number proposed to connect the sensor or I/O module</param>
        /// <returns>this function returns the special value of desired Input module</returns>
        private void Read_param_request(byte Tool_Name, byte Tool_ID)
        {
            int checksum = 0;
            if (SD_Com_en)
            {
                Tx_tmp[0] = header;
                Tx_tmp[1] = header;
                Tx_tmp[2] = Tool_Name;
                Tx_tmp[3] = Tool_ID;

                checksum = Tool_Name + Tool_ID;
                Tx_tmp[4] = (byte)checksum;
                Tx_tmp[5] = (byte)(checksum >> 8);

                try
                {
                    SD_Com.Write(Tx_tmp, 0, 6);
                }
                catch
                {
                    //MessageBox.Show("Error in request !!!");  
                }
            }

            SD_Com.DiscardOutBuffer();
            SD_Com.DiscardInBuffer();
            Thread.Sleep(10);
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tool_Name"></param>
        /// <param name="Tool_ID"></param>
        /// <returns></returns>
        private Int16 Read_param_answer(byte Tool_Name, byte Tool_ID)
        {
            int _checksum = 0;
            int checksum = 0;
             try
             {
                 SD_Com.ReadTimeout = 10;
                 if ((byte)SD_Com.ReadByte() == header)
                 {
                     if ((byte)SD_Com.ReadByte() == header)
                     {
                         if ((byte)SD_Com.ReadByte() == Tool_Name)
                         {
                             if ((byte)SD_Com.ReadByte() == Tool_ID)
                             {
                                 tmp_sen[0] = (byte)SD_Com.ReadByte();
                             }

                             checksum = Tool_Name + Tool_ID + tmp_sen[0];

                             tmp_sen[1] = (byte)SD_Com.ReadByte();
                             tmp_sen[2] = (byte)SD_Com.ReadByte();

                             _checksum = (tmp_sen[2] << 8) + tmp_sen[1];
                         }
                     }
                 }
             }
             catch
             {
                 // MessageBox.Show("Error in Read !!!");
             }

             SD_Com.DiscardOutBuffer();
             SD_Com.DiscardInBuffer();
             if (_checksum != checksum)
             {
                return 300;
             }
             else return tmp_sen[0];
        }
        //***********************************************************************************
        /// <summary>
        /// This function used for Split the desired Word to its letters
        /// </summary>
        /// <param name="word">Input the String that we want to split it to its letters</param>
        private void SplitString(string word)
        {
            charArray = word.ToCharArray();
        }
        //***********************************************************************************
        /// <summary>
        /// This finction handels the Read progress form I/Os from PRC 
        /// </summary>
        private void Read_Thread()
        {
            if (SD_Com_en)
            {
                Thread Read_Thread = new Thread(new ParameterizedThreadStart(
                    new Action<object>((t) =>
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                        while (thread_enable)
                        {
                            Thread.Sleep(5);
                            if (_Sensor_Enable[0] == 1)
                            {
                                Read_param_request(_Tool_Name[0], 0x01);
                                dummy = Read_param_answer(_Tool_Name[0], 0x01);
                                if (dummy != 300) res_tSen[0] = dummy;
                            }
                            if (_Sensor_Enable[1] == 1)
                            {
                                Read_param_request(_Tool_Name[1], 0x02);
                                dummy = Read_param_answer(_Tool_Name[1], 0x02);
                                if (dummy != 300) res_tSen[1] = dummy;
                            }
                            if (_Sensor_Enable[0] == 1)
                            {
                                Read_param_request(_Tool_Name[2], 0x03);
                                dummy = Read_param_answer(_Tool_Name[2], 0x03);
                                if (dummy != 300) res_tSen[2] = dummy;
                            }
                            if (_Sensor_Enable[0] == 1)
                            {
                                Read_param_request(_Tool_Name[3], 0x04);
                                dummy = Read_param_answer(_Tool_Name[3], 0x04);
                                if (dummy != 300) res_tSen[3] = dummy;
                            }
                            if (_Sensor_Enable[0] == 1)
                            {
                                Read_param_request(_Tool_Name[4], 0x05);
                                dummy = Read_param_answer(_Tool_Name[4], 0x05);
                                if (dummy != 300) res_tSen[4] = dummy;
                            }
                            if (_Sensor_Enable[0] == 1)
                            {
                                Read_param_request(_Tool_Name[5], 0x06);
                                dummy = Read_param_answer(_Tool_Name[5], 0x06);
                                if (dummy != 300) res_tSen[5] = dummy;
                            }
                            if (_Sensor_Enable[0] == 1)
                            {
                                Read_param_request(_Tool_Name[6], 0x07);
                                dummy = Read_param_answer(_Tool_Name[6], 0x07);
                                if (dummy != 300) res_tSen[6] = dummy;
                            }
                            if (_Sensor_Enable[0] == 1)
                            {
                                Read_param_request(_Tool_Name[7], 0x08);
                                dummy = Read_param_answer(_Tool_Name[7], 0x08);
                                if (dummy != 300) res_tSen[7] = dummy;
                            }
                        }
                    })));

                Read_Thread.SetApartmentState(ApartmentState.STA);
                Read_Thread.Start();
            }
            else
            {
                MessageBox.Show("Please Open Com-port");
            }
        }
        //***********************************************************************************
        /// <summary>
        /// This function Rotates an image in a spesific pictureBox throght EMGUCVs methods
        /// </summary>
        /// <param name="img"></param>
        /// <param name="angle"></param>
        /// <param name="PB"></param>
        public void Image_RotateEMGU(Image<Bgr, byte> img, int angle, PictureBox PB)
        {
            PB.Image = (img.Rotate(angle, new Bgr(Color.Transparent))).ToBitmap();  
        }
        //***********************************************************************************
        /// <summary>
        /// this function extracts the polar position from scaler coordinates 
        /// </summary>
        /// <param name="Mouse_Pos"></param>
        private float Polar_Cordinate_Extender(PointF Mouse_Pos)
        {
            if (Mouse_Pos.Y == 0 && Mouse_Pos.X > 0) _angle = 0;
            else if (Mouse_Pos.Y == 0 && Mouse_Pos.X < 0) _angle = 180;
            else if (Mouse_Pos.Y > 0 && Mouse_Pos.X == 0) _angle = 90;
            else if (Mouse_Pos.Y < 0 && Mouse_Pos.X == 0) _angle = 270;
            else _angle = (float)(Math.Atan2(Mouse_Pos.Y, Mouse_Pos.X) * (180 / Math.PI));
            return _angle;
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Beeper_btn_Click(object sender, EventArgs e)
        {
            byte tmp = 0;
            tmp = (byte)num_BRC.Value; 
            if ((byte)num_BRC.Value == 0) tmp = 1;

            Set_param((byte)0x15, (byte)tmp, (byte)(num_BONt.Value * 10), (byte)(num_BOFFt.Value * 10), 0);
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;
            
            // Confirm user wants to close
            switch (MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    e.Cancel = true;
                    break;
                default:
                    if (SD_Com_en) Send_close_command();
                    Image_R.Clone();
                    Thread.Sleep(50);
                    Image_R.Clone();
                    Thread.Sleep(50);
                    thread_enable = false;
                    if (New_form_ex)
                    {
                        NewForm.Dispose(true);
                    }
                    
                    break;
            }
        }
        //**********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        private void Image_Roate()
        {
            Image_R=new Action(()=>{
            
                while (thread_enable)
                {
                    Thread.Sleep(5);
                    if (Send_first[0] == 1)
                    {
                        _tmp_angle[0] += _Motor_Speed[0] / 10;
                        RotateImage(pb_W1, (Bitmap)Properties.Resources.pully, _tmp_angle[0]);
                    }
                    if (Send_first[1] == 1)
                    {
                        _tmp_angle[1] += _Motor_Speed[1] / 10;
                        RotateImage(pb_W2, (Bitmap)Properties.Resources.pully, _tmp_angle[1]);
                    }
                    if (Send_first[2] == 1)
                    {
                        _tmp_angle[2] += _Motor_Speed[2] / 10;
                        RotateImage(pb_W3, (Bitmap)Properties.Resources.pully, _tmp_angle[2]);
                    }
                    if (Send_first[3] == 1)
                    {
                        _tmp_angle[3] += _Motor_Speed[3] / 10;
                        RotateImage(pb_W4, (Bitmap)Properties.Resources.pully, _tmp_angle[3]);
                    }
                }
            });
            Image_R.BeginInvoke(null, null);
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            switch (MessageBox.Show(this, "Are you sure you want to reset the application?", "Reseting", MessageBoxButtons.YesNo))
            {
                case DialogResult.No:
                    break;
                default:
                    if (SD_Com_en)  Send_close_command();

                    thread_enable = false;
                    Image_R.Clone();
                    Thread.Sleep(50);
                    Image_R.Clone();
                    Thread.Sleep(50);
                    NewForm = new Rrealtime_Debugger();
                    NewForm.Show();
                    New_form_ex = true;
                    this.Dispose(false);

                    break;
            }
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GUI_update_Tick(object sender, EventArgs e)
        {
            lbl_ADC1.Text = res_tSen[0].ToString();
            lbl_ADC2.Text = res_tSen[1].ToString();
            lbl_ADC3.Text = res_tSen[2].ToString();
            lbl_ADC4.Text = res_tSen[3].ToString();
            lbl_ADC5.Text = res_tSen[4].ToString();
            lbl_ADC6.Text = res_tSen[5].ToString();
            lbl_ADC7.Text = res_tSen[6].ToString();
            lbl_ADC8.Text = res_tSen[7].ToString();
        }


        //***********************************************************************************
        #endregion
        
    }
}
