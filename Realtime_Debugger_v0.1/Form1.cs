using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;

using RotatePictureBox;
using Utility.ModifyRegistry;


using Emgu.CV;
using Emgu.CV.Structure;

namespace Realtime_Debugger_v0._1
{
    public partial class Rrealtime_Debugger : Form
    {
        #region Defenitions

        #region Toolnames
        private enum Toolname : byte
        {
            INVALID = 0x00,
            BUZZER = 0x15,
            SOUND_SENSOR = 0x25,
            LIGHT_SENSOR = 0x25,
            IR_SENSOR = 0x45,
            DC_MOTOR = 0x51,
            SERVO_MOTOR = 0x52,
            LED = 0x54,
            LCD = 0x58,
            TOUCH_SENSOR = 0x85
        }
        #endregion

        #region Packet
        private const byte HEADER = 0xAA;
        #endregion
        
        #region DC Motors
        private enum DC_MOTORS_ID : byte 
        {
            DC01 = 0x01,
            DC02 = 0x02,
            DC03 = 0x03,
            DC04 = 0x04,
            DCALL = 0x05
        }
        private enum DC_MOTORS_DIR : byte
        {
            CW = 0x00,
            CCW = 0x01
        }
        #endregion
        
        #region Servo Motors
        private enum SERVOS_ID : byte
        {
            SERVO01 = 0x01,
            SERVO02 = 0x02,
            SERVO03 = 0x03,
            SERVO04 = 0x04
        }
        private enum SERVO_COMMAND : byte
        {
            OFF = 0xC8
        }
        #endregion

        #region AD Ports
        private enum ADs_ID : byte
        {
            AD01 = 0x01,
            AD02 = 0x02,
            AD03 = 0x03,
            AD04 = 0x04,
            AD05 = 0x05,
            AD06 = 0x06,
            AD07 = 0x07,
            AD08 = 0x08
        }
        #endregion

        #region LEDs
        private enum LEDs_ID : byte
        {
            LED01 = 0x01,
            LED02 = 0x02,
            LED03 = 0x03,
            LED04 = 0x04,
            LED05 = 0x05,
            LED06 = 0x06,
            LED07 = 0x07,
            LED08 = 0x08,
            LED09 = 0x09,
            LED10 = 0x0A
        }
        private enum LEDs_State : byte
        {
            OFF = 0x00,
            ON = 0x01
        }
        #endregion

        SerialPort SD_Com;
        ModifyRegistry User_des = new ModifyRegistry();
        Action Image_R;
        Thread Read_Req_Th;
        Thread Read_Th;
        byte[] LED_S = new byte[2];
        int[] _tmp_angle = new int[4];
        bool Read_Req_Th_ex = false;
        bool Read_Th_ex = false;
        private Bitmap image = null;
        private const byte Try_Num = 5;
        private byte[] empty=new byte[8];
        byte[] Tx_tmp = new byte[20];
        bool thread_enable = true;
        bool[] S_en=new bool[4];
        bool SD_Com_en = false;
        Int16[] res_tSen = new Int16[8];
        int[] Motor_Speed = new int[4];
        int[] _Motor_Speed = new int[4];
        DC_MOTORS_DIR[] Motor_direction = new DC_MOTORS_DIR[4];
        char[] charArray;
        byte[] _tmp_Splitedstring = new byte[8];
        byte backlight = 1;
        Toolname[] _Tool_Name = new Toolname[8];
        byte[] _Sensor_Enable=new byte[8];
        byte[] Send_first = new byte[4];
        #endregion

        public Rrealtime_Debugger()
        {
            
            InitializeComponent();

            LED_S[0] = 0;
            LED_S[1] = 0;

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
            toolStripComboBox1.Items.Add("Auto Search");
            toolStripComboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            if (System.IO.Ports.SerialPort.GetPortNames().Any(x => x == User_des.Read("User_Com")))
                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf(User_des.Read("User_Com"));
            else
                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf("Auto Search");

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
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("Auto Search");
            toolStripComboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            if (User_des.Read("User_Com") != "" &&
                System.IO.Ports.SerialPort.GetPortNames().Any(x => x == User_des.Read("User_Com")))
            {
                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf(User_des.Read("User_Com"));
            }
            else
            {
                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf("Auto Search");
            }
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
                cmb_LED1.SelectedIndex = -1;
                cmb_LED1.Visible = false;
                _Sensor_Enable[0] = 0;
                _Tool_Name[0] = Toolname.INVALID;
            }
            else if (com_ADC1.Text == "LED Light")
            {
                pb_ADC1.Image = Properties.Resources.LED2;
                lbl_SADC1.Text = "Status";
                lbl_SADC1.Visible = true;
                lbl_ADC1.Visible = false;
                cmb_LED1.Text = "Off";
                cmb_LED1.Visible = true;
                _Sensor_Enable[0] = 0;
                _Tool_Name[0] = Toolname.LED;
            }
            else
            {
                if (com_ADC1.Text == "Touch Sensor")
                {
                    pb_ADC1.Image = Properties.Resources.T;
                    _Tool_Name[0] = Toolname.TOUCH_SENSOR;
                }
                if (com_ADC1.Text == "Light Sensor")
                {
                    pb_ADC1.Image = Properties.Resources.LS;
                    _Tool_Name[0] = Toolname.LIGHT_SENSOR;
                }
                if (com_ADC1.Text == "Sound Sensor")
                {
                    pb_ADC1.Image = Properties.Resources.SS;
                    _Tool_Name[0] = Toolname.SOUND_SENSOR;
                }
                if (com_ADC1.Text == "Infrared Sensor")
                {
                    pb_ADC1.Image = Properties.Resources.IR;
                    _Tool_Name[0] = Toolname.IR_SENSOR;
                }

                lbl_SADC1.Text = "Value";
                lbl_SADC1.Visible = true;
                lbl_ADC1.Visible = true;
                cmb_LED1.SelectedIndex = -1;
                cmb_LED1.Visible = false;
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
                cmb_LED2.SelectedIndex = -1;
                cmb_LED2.Visible = false;
                _Sensor_Enable[1] = 0;
                _Tool_Name[1] = Toolname.INVALID;
            }
            else if (com_ADC2.Text == "LED Light")
            {
                pb_ADC2.Image = Properties.Resources.LED2;
                lbl_SADC2.Text = "Status";
                lbl_SADC2.Visible = true;
                lbl_ADC2.Visible = false;
                cmb_LED2.Text = "Off";
                cmb_LED2.Visible = true;
                _Sensor_Enable[1] = 0;
                _Tool_Name[1] = Toolname.LED;
            }
            else
            {
                if (com_ADC2.Text == "Touch Sensor")
                {
                    pb_ADC2.Image = Properties.Resources.T;
                    _Tool_Name[1] = Toolname.TOUCH_SENSOR;
                }
                if (com_ADC2.Text == "Light Sensor")
                {
                    pb_ADC2.Image = Properties.Resources.LS;
                    _Tool_Name[1] = Toolname.LIGHT_SENSOR;
                }
                if (com_ADC2.Text == "Sound Sensor")
                {
                    pb_ADC2.Image = Properties.Resources.SS;
                    _Tool_Name[1] = Toolname.SOUND_SENSOR;
                }
                if (com_ADC2.Text == "Infrared Sensor")
                {
                    pb_ADC2.Image = Properties.Resources.IR;
                    _Tool_Name[1] = Toolname.IR_SENSOR;
                }

                lbl_SADC2.Text = "Value";
                lbl_SADC2.Visible = true;
                lbl_ADC2.Visible = true;
                cmb_LED2.SelectedIndex = -1;
                cmb_LED2.Visible = false;
                _Sensor_Enable[1] = 1;
            }
        }

        private void com_ADC3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC3.Text == "   ")
            {
                pb_ADC3.Image = null;
                lbl_SADC3.Visible = false;
                lbl_ADC3.Visible = false;
                cmb_LED3.SelectedIndex = -1;
                cmb_LED3.Visible = false;
                _Sensor_Enable[2] = 0;
                _Tool_Name[2] = Toolname.INVALID;
            }
            else if (com_ADC3.Text == "LED Light")
            {
                pb_ADC3.Image = Properties.Resources.LED2;
                lbl_SADC3.Text = "Status";
                lbl_SADC3.Visible = true;
                lbl_ADC3.Visible = false;
                cmb_LED3.Text = "Off";
                cmb_LED3.Visible = true;
                _Sensor_Enable[2] = 0;
                _Tool_Name[2] = Toolname.LED;
            }
            else
            {
                if (com_ADC3.Text == "Touch Sensor")
                {
                    pb_ADC3.Image = Properties.Resources.T;
                    _Tool_Name[2] = Toolname.TOUCH_SENSOR;
                }
                if (com_ADC3.Text == "Light Sensor")
                {
                    pb_ADC3.Image = Properties.Resources.LS;
                    _Tool_Name[2] = Toolname.LIGHT_SENSOR;
                }
                if (com_ADC3.Text == "Sound Sensor")
                {
                    pb_ADC3.Image = Properties.Resources.SS;
                    _Tool_Name[2] = Toolname.SOUND_SENSOR;
                }
                if (com_ADC3.Text == "Infrared Sensor")
                {
                    pb_ADC3.Image = Properties.Resources.IR;
                    _Tool_Name[2] = Toolname.IR_SENSOR;
                }

                lbl_SADC3.Text = "Value";
                lbl_SADC3.Visible = true;
                lbl_ADC3.Visible = true;
                cmb_LED3.SelectedIndex = -1;
                cmb_LED3.Visible = false;
                _Sensor_Enable[2] = 1;
            }
        }

        private void com_ADC4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC4.Text == "   ")
            {
                pb_ADC4.Image = null;
                lbl_SADC4.Visible = false;
                lbl_ADC4.Visible = false;
                cmb_LED4.SelectedIndex = -1;
                cmb_LED4.Visible = false;
                _Sensor_Enable[3] = 0;
                _Tool_Name[3] = Toolname.INVALID;
            }
            else if (com_ADC4.Text == "LED Light")
            {
                pb_ADC4.Image = Properties.Resources.LED2;
                lbl_SADC4.Text = "Status";
                lbl_SADC4.Visible = true;
                lbl_ADC4.Visible = false;
                cmb_LED4.Text = "Off";
                cmb_LED4.Visible = true;
                _Sensor_Enable[3] = 0;
                _Tool_Name[3] = Toolname.LED;
            }
            else
            {
                if (com_ADC4.Text == "Touch Sensor")
                {
                    pb_ADC4.Image = Properties.Resources.T;
                    _Tool_Name[3] = Toolname.TOUCH_SENSOR;
                }
                if (com_ADC4.Text == "Light Sensor")
                {
                    pb_ADC4.Image = Properties.Resources.LS;
                    _Tool_Name[3] = Toolname.LIGHT_SENSOR;
                }
                if (com_ADC4.Text == "Sound Sensor")
                {
                    pb_ADC4.Image = Properties.Resources.SS;
                    _Tool_Name[3] = Toolname.SOUND_SENSOR;
                }
                if (com_ADC4.Text == "Infrared Sensor")
                {
                    pb_ADC4.Image = Properties.Resources.IR;
                    _Tool_Name[3] = Toolname.IR_SENSOR;
                }

                lbl_SADC4.Text = "Value";
                lbl_SADC4.Visible = true;
                lbl_ADC4.Visible = true;
                cmb_LED4.SelectedIndex = -1;
                cmb_LED4.Visible = false;
                _Sensor_Enable[3] = 1;
            }
        }

        private void com_ADC5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (com_ADC5.Text == "   ")
            {
                pb_ADC5.Image = null;
                lbl_SADC5.Visible = false;
                lbl_ADC5.Visible = false;
                cmb_LED5.SelectedIndex = -1;
                cmb_LED5.Visible = false;
                _Sensor_Enable[4] = 0;
                _Tool_Name[4] = Toolname.INVALID;
            }
            else if (com_ADC5.Text == "LED Light")
            {
                pb_ADC5.Image = Properties.Resources.LED2;
                lbl_SADC5.Text = "Status";
                lbl_SADC5.Visible = true;
                lbl_ADC5.Visible = false;
                cmb_LED5.Text = "Off";
                cmb_LED5.Visible = true;
                _Sensor_Enable[4] = 0;
                _Tool_Name[4] = Toolname.LED;
            }
            else
            {
                if (com_ADC5.Text == "Touch Sensor")
                {
                    pb_ADC5.Image = Properties.Resources.T;
                    _Tool_Name[4] = Toolname.TOUCH_SENSOR;
                }
                if (com_ADC5.Text == "Light Sensor")
                {
                    pb_ADC5.Image = Properties.Resources.LS;
                    _Tool_Name[4] = Toolname.LIGHT_SENSOR;
                }
                if (com_ADC5.Text == "Sound Sensor")
                {
                    pb_ADC5.Image = Properties.Resources.SS;
                    _Tool_Name[4] = Toolname.SOUND_SENSOR;
                }
                if (com_ADC5.Text == "Infrared Sensor")
                {
                    pb_ADC5.Image = Properties.Resources.IR;
                    _Tool_Name[4] = Toolname.IR_SENSOR;
                }

                lbl_SADC5.Text = "Value";
                lbl_SADC5.Visible = true;
                lbl_ADC5.Visible = true;
                cmb_LED5.SelectedIndex = -1;
                cmb_LED5.Visible = false;
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
                cmb_LED6.SelectedIndex = -1;
                cmb_LED6.Visible = false;
                _Sensor_Enable[5] = 0;
                _Tool_Name[5] = Toolname.INVALID;
            }
            else if (com_ADC6.Text == "LED Light")
            {
                pb_ADC6.Image = Properties.Resources.LED2;
                lbl_SADC6.Text = "Status";
                lbl_SADC6.Visible = true;
                lbl_ADC6.Visible = false;
                cmb_LED6.Text = "Off";
                cmb_LED6.Visible = true;
                _Sensor_Enable[5] = 0;
                _Tool_Name[5] = Toolname.LED;
            }
            else
            {
                if (com_ADC6.Text == "Touch Sensor")
                {
                    pb_ADC6.Image = Properties.Resources.T;
                    _Tool_Name[5] = Toolname.TOUCH_SENSOR;
                }
                if (com_ADC6.Text == "Light Sensor")
                {
                    pb_ADC6.Image = Properties.Resources.LS;
                    _Tool_Name[5] = Toolname.LIGHT_SENSOR;
                }
                if (com_ADC6.Text == "Sound Sensor")
                {
                    pb_ADC6.Image = Properties.Resources.SS;
                    _Tool_Name[5] = Toolname.SOUND_SENSOR;
                }
                if (com_ADC6.Text == "Infrared Sensor")
                {
                    pb_ADC6.Image = Properties.Resources.IR;
                    _Tool_Name[5] = Toolname.IR_SENSOR;
                }

                lbl_SADC6.Text = "Value";
                lbl_SADC6.Visible = true;
                lbl_ADC6.Visible = true;
                cmb_LED6.SelectedIndex = -1;
                cmb_LED6.Visible = false;
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
                cmb_LED7.SelectedIndex = -1;
                cmb_LED7.Visible = false;
                _Sensor_Enable[6] = 0;
                _Tool_Name[6] = Toolname.INVALID;
            }
            else if (com_ADC7.Text == "LED Light")
            {
                pb_ADC7.Image = Properties.Resources.LED2;
                lbl_SADC7.Text = "Status";
                lbl_SADC7.Visible = true;
                lbl_ADC7.Visible = false;
                cmb_LED7.Text = "Off";
                cmb_LED7.Visible = true;
                _Sensor_Enable[6] = 0;
                _Tool_Name[6] = Toolname.LED;
            }
            else
            {
                if (com_ADC7.Text == "Touch Sensor")
                {
                    pb_ADC7.Image = Properties.Resources.T;
                    _Tool_Name[6] = Toolname.TOUCH_SENSOR;
                }
                if (com_ADC7.Text == "Light Sensor")
                {
                    pb_ADC7.Image = Properties.Resources.LS;
                    _Tool_Name[6] = Toolname.LIGHT_SENSOR;
                }
                if (com_ADC7.Text == "Sound Sensor")
                {
                    pb_ADC7.Image = Properties.Resources.SS;
                    _Tool_Name[6] = Toolname.SOUND_SENSOR;
                }
                if (com_ADC7.Text == "Infrared Sensor")
                {
                    pb_ADC7.Image = Properties.Resources.IR;
                    _Tool_Name[6] = Toolname.IR_SENSOR;
                }

                lbl_SADC7.Text = "Value";
                lbl_SADC7.Visible = true;
                lbl_ADC7.Visible = true;
                cmb_LED7.SelectedIndex = -1;
                cmb_LED7.Visible = false;
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
                cmb_LED8.SelectedIndex = -1;
                cmb_LED8.Visible = false;
                _Sensor_Enable[7] = 0;
                _Tool_Name[7] = Toolname.INVALID;
            }
            else if (com_ADC8.Text == "LED Light")
            {
                pb_ADC8.Image = Properties.Resources.LED2;
                lbl_SADC8.Text = "Status";
                lbl_SADC8.Visible = true;
                lbl_ADC8.Visible = false;
                cmb_LED8.Text = "Off";
                cmb_LED8.Visible = true;
                _Sensor_Enable[7] = 0;
                _Tool_Name[7] = Toolname.LED;
            }
            else
            {
                if (com_ADC8.Text == "Touch Sensor")
                {
                    pb_ADC8.Image = Properties.Resources.T;
                    _Tool_Name[7] = Toolname.TOUCH_SENSOR;
                }
                if (com_ADC8.Text == "Light Sensor")
                {
                    pb_ADC8.Image = Properties.Resources.LS;
                    _Tool_Name[7] = Toolname.LIGHT_SENSOR;
                }
                if (com_ADC8.Text == "Sound Sensor")
                {
                    pb_ADC8.Image = Properties.Resources.SS;
                    _Tool_Name[7] = Toolname.SOUND_SENSOR;
                }
                if (com_ADC8.Text == "Infrared Sensor")
                {
                    pb_ADC8.Image = Properties.Resources.IR;
                    _Tool_Name[7] = Toolname.IR_SENSOR;
                }

                lbl_SADC8.Text = "Value";
                lbl_SADC8.Visible = true;
                lbl_ADC8.Visible = true;
                cmb_LED8.SelectedIndex = -1;
                cmb_LED8.Visible = false;
                _Sensor_Enable[7] = 1;
            }
        }

        #endregion

        #region DC Motors
        private void btn_M1_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                trb_SM1.Value = 0;
                _Motor_Speed[0] = Motor_Speed[0];
                Send_first[0] = 0;
                num_TimeM1.Value = 0;
                TM1.Dispose();
                Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC01, 0x00, 0x00, 0x00);
            }
        }

        private void btn_M2_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                trb_SM2.Value = 0;
                _Motor_Speed[1] = Motor_Speed[1];
                Send_first[1] = 0;
                num_TimeM2.Value = 0;
                TM2.Dispose();
                Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC02, 0x00, 0x00, 0x00);
            }
        }

        private void btn_M3_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                trb_SM3.Value = 0;
                _Motor_Speed[2] = Motor_Speed[2];
                Send_first[2] = 0;
                num_TimeM3.Value = 0;
                TM3.Dispose();
                Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC03, 0x00, 0x00, 0x00);
            }
        }

        private void btn_M4_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                trb_SM4.Value = 0;
                _Motor_Speed[3] = Motor_Speed[3];
                Send_first[3] = 0;
                num_TimeM4.Value = 0;
                TM4.Dispose();
                Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC04, 0x00, 0x00, 0x00);
            }
        }

        private void btn_SyncSendM_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                if (cmb_SyncDM.SelectedIndex != -1)
                {
                    DC_MOTORS_DIR tempDirection = DC_MOTORS_DIR.CW;
                    if (cmb_SyncDM.Text == "CW")
                    {
                        tempDirection = DC_MOTORS_DIR.CW;
                        trb_SM1.Value = (int)(num_SyncSM.Value);
                        trb_SM2.Value = (int)(num_SyncSM.Value);
                        trb_SM3.Value = (int)(num_SyncSM.Value);
                        trb_SM4.Value = (int)(num_SyncSM.Value);
                    }
                    
                    if (cmb_SyncDM.Text == "CCW")
                    {
                        tempDirection = DC_MOTORS_DIR.CCW;
                        trb_SM1.Value = -(int)(num_SyncSM.Value);
                        trb_SM2.Value = -(int)(num_SyncSM.Value);
                        trb_SM3.Value = -(int)(num_SyncSM.Value);
                        trb_SM4.Value = -(int)(num_SyncSM.Value);
                    }

                    num_TimeM1.Value = num_SyncTM.Value;
                    num_TimeM2.Value = num_SyncTM.Value;
                    num_TimeM3.Value = num_SyncTM.Value;
                    num_TimeM4.Value = num_SyncTM.Value;

                    for (byte cnt = 0; cnt < 4; cnt++)
                    {
                        _Motor_Speed[cnt] = Motor_Speed[cnt];
                        Send_first[cnt] = 1;
                    }

                    if (num_SyncTM.Value != 0)
                    {
                        SyncTimer.Interval = (int)(num_SyncTM.Value * 1000);
                        SyncTimer.Enabled = true;
                    }

                    Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DCALL, (byte)(num_SyncSM.Value), (byte)tempDirection, (byte)(num_SyncTM.Value * 10));
                }
                else
                {
                    MessageBox.Show(this, "Please select the direction.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btn_SyncStopM_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                num_SyncSM.Value = 0;
                trb_SM1.Value = 0;
                trb_SM2.Value = 0;
                trb_SM3.Value = 0;
                trb_SM4.Value = 0;

                num_SyncTM.Value = 0;
                num_TimeM1.Value = 0;
                num_TimeM2.Value = 0;
                num_TimeM3.Value = 0;
                num_TimeM4.Value = 0;

                for (byte cnt = 0; cnt < 4; cnt++)
                {
                    _Motor_Speed[cnt] = Motor_Speed[cnt];
                    Send_first[cnt] = 0;
                }

                SyncTimer.Dispose();

                Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DCALL, 0x00, 0x00, 0x00);
            }
        }

        private void SyncTimer_Tick(object sender, EventArgs e)
        {
            SyncTimer.Enabled = false;

            num_SyncSM.Value = 0;
            trb_SM1.Value = 0;
            trb_SM2.Value = 0;
            trb_SM3.Value = 0;
            trb_SM4.Value = 0;

            num_SyncTM.Value = 0;
            num_TimeM1.Value = 0;
            num_TimeM2.Value = 0;
            num_TimeM3.Value = 0;
            num_TimeM4.Value = 0;

            for (byte cnt = 0; cnt < 4; cnt++)
            {
                _Motor_Speed[cnt] = Motor_Speed[cnt];
                Send_first[cnt] = 0;
            }

            Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DCALL, 0x00, 0x00, 0x00);
        }

        private void trb_SM1_ValueChanged(object sender, EventArgs e)
        {
            Motor_Speed[0] = trb_SM1.Value;

            if (Motor_Speed[0] > 0) 
            {
                Motor_direction[0] = DC_MOTORS_DIR.CW;
                tb_StM1.Text = DC_MOTORS_DIR.CW.ToString(); 
            }
            else if (Motor_Speed[0] < 0) 
            {
                Motor_direction[0] = DC_MOTORS_DIR.CCW;
                tb_StM1.Text = DC_MOTORS_DIR.CCW.ToString(); 
            }
            else 
                tb_StM1.Text = "Stop";

            lbl_SM1.Text = Math.Abs(Motor_Speed[0]).ToString();
        }

        private void trb_SM2_ValueChanged(object sender, EventArgs e)
        {
            Motor_Speed[1] = trb_SM2.Value;

            if (Motor_Speed[1] > 0) 
            {
                Motor_direction[1] = DC_MOTORS_DIR.CW;
                tb_StM2.Text = DC_MOTORS_DIR.CW.ToString(); 
            }
            else if (Motor_Speed[1] < 0) 
            {
                Motor_direction[1] = DC_MOTORS_DIR.CCW;
                tb_StM2.Text = DC_MOTORS_DIR.CCW.ToString();
            }
            else 
                tb_StM2.Text = "Stop";

            lbl_SM2.Text = Math.Abs(Motor_Speed[1]).ToString();
        }

        private void trb_SM3_ValueChanged(object sender, EventArgs e)
        {
            Motor_Speed[2] = trb_SM3.Value;

            if (Motor_Speed[2] > 0)
            {
                Motor_direction[2] = DC_MOTORS_DIR.CW;
                tb_StM3.Text = DC_MOTORS_DIR.CW.ToString(); 
            }
            else if (Motor_Speed[2] < 0) 
            {
                Motor_direction[2] = DC_MOTORS_DIR.CCW;
                tb_StM3.Text = DC_MOTORS_DIR.CCW.ToString(); 
            }
            else
                tb_StM3.Text = "Stop";

            lbl_SM3.Text = Math.Abs(Motor_Speed[2]).ToString();
        }

        private void trb_SM4_ValueChanged(object sender, EventArgs e)
        {
            Motor_Speed[3] = trb_SM4.Value;

            if (Motor_Speed[3] > 0) 
            {
                Motor_direction[3] = DC_MOTORS_DIR.CW;
                tb_StM4.Text = DC_MOTORS_DIR.CW.ToString(); 
            }
            else if (Motor_Speed[3] < 0) 
            {
                Motor_direction[3] = DC_MOTORS_DIR.CCW; 
                tb_StM4.Text = DC_MOTORS_DIR.CCW.ToString(); 
            }
            else 
                tb_StM4.Text = "Stop";

            lbl_SM4.Text = Math.Abs(Motor_Speed[3]).ToString();
        }

        private void btn_SendM1_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                _Motor_Speed[0] = Motor_Speed[0];
                Send_first[0] = 1;

                if (num_TimeM1.Value != 0)
                {
                    TM1.Interval = (int)(num_TimeM1.Value * 1000);
                    TM1.Enabled = true;
                }

                Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC01, (byte)(Math.Abs(Motor_Speed[0])), (byte)Motor_direction[0], (byte)(num_TimeM1.Value * 10));
            }
        }

        private void btn_SendM2_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                _Motor_Speed[1] = Motor_Speed[1];
                Send_first[1] = 1;

                if (num_TimeM2.Value != 0)
                {
                    TM2.Interval = (int)(num_TimeM2.Value * 1000);
                    TM2.Enabled = true;
                }

                Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC02, (byte)(Math.Abs(Motor_Speed[1])), (byte)Motor_direction[1], (byte)(num_TimeM2.Value * 10));
            }
        }

        private void btn_SendM3_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                _Motor_Speed[2] = Motor_Speed[2];
                Send_first[2] = 1;

                if (num_TimeM3.Value != 0)
                {
                    TM3.Interval = (int)(num_TimeM3.Value * 1000);
                    TM3.Enabled = true;
                }

                Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC03, (byte)(Math.Abs(Motor_Speed[2])), (byte)Motor_direction[2], (byte)(num_TimeM3.Value * 10));
            }
        }

        private void btn_SendM4_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                _Motor_Speed[3] = Motor_Speed[3];
                Send_first[3] = 1;

                if (num_TimeM4.Value != 0)
                {
                    TM4.Interval = (int)(num_TimeM4.Value * 1000);
                    TM4.Enabled = true;
                }

                Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC04, (byte)(Math.Abs(Motor_Speed[3])), (byte)Motor_direction[3], (byte)(num_TimeM4.Value * 10));
            }
        }

        private void TM1_Tick(object sender, EventArgs e)
        {
            TM1.Enabled = false;
            trb_SM1.Value = 0;
            _Motor_Speed[0] = Motor_Speed[0];
            Send_first[0] = 0;
            num_TimeM1.Value = 0;
            Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC01, 0x00, 0x00, 0x00);
        }

        private void TM2_Tick(object sender, EventArgs e)
        {
            TM2.Enabled = false;
            trb_SM2.Value = 0;
            _Motor_Speed[1] = Motor_Speed[1];
            Send_first[1] = 0;
            num_TimeM2.Value = 0;
            Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC02, 0x00, 0x00, 0x00);
        }

        private void TM3_Tick(object sender, EventArgs e)
        {
            TM3.Enabled = false;
            trb_SM3.Value = 0;
            _Motor_Speed[2] = Motor_Speed[2];
            Send_first[2] = 0;
            num_TimeM3.Value = 0;
            Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC03, 0x00, 0x00, 0x00);
        }

        private void TM4_Tick(object sender, EventArgs e)
        {
            TM4.Enabled = false;
            trb_SM4.Value = 0;
            _Motor_Speed[3] = Motor_Speed[3];
            Send_first[3] = 0;
            num_TimeM4.Value = 0;
            Set_param(Toolname.DC_MOTOR, (byte)DC_MOTORS_ID.DC04, 0x00, 0x00, 0x00);
        }

        #endregion

        #region LED Conf

        private void btn_AllSensors_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                if (cmb_Allsensors.Text == "Clear")
                {
                    com_ADC1.Text = "   ";
                    com_ADC2.Text = "   ";
                    com_ADC3.Text = "   ";
                    com_ADC4.Text = "   ";
                    com_ADC5.Text = "   ";
                    com_ADC6.Text = "   ";
                    com_ADC7.Text = "   ";
                    com_ADC8.Text = "   ";
                }
                else if( cmb_Allsensors.SelectedIndex != -1 )
                {
                    com_ADC1.Text = cmb_Allsensors.Text;
                    com_ADC2.Text = cmb_Allsensors.Text;
                    com_ADC3.Text = cmb_Allsensors.Text;
                    com_ADC4.Text = cmb_Allsensors.Text;
                    com_ADC5.Text = cmb_Allsensors.Text;
                    com_ADC6.Text = cmb_Allsensors.Text;
                    com_ADC7.Text = cmb_Allsensors.Text;
                    com_ADC8.Text = cmb_Allsensors.Text;
                }
                else
                {
                    MessageBox.Show(this, "Please select the device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cmb_LED1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED1.Text == "ON") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED01, (byte)LEDs_State.ON, 0x00, 0x00);
            if (cmb_LED1.Text == "OFF")
                Set_param(Toolname.LED, (byte)LEDs_ID.LED01, (byte)LEDs_State.OFF, 0x00, 0x00);
        }

        private void cmb_LED2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED2.Text == "ON") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED02, (byte)LEDs_State.ON, 0x00, 0x00);
            if (cmb_LED2.Text == "OFF") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED02, (byte)LEDs_State.OFF, 0x00, 0x00);
        }

        private void cmb_LED3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED3.Text == "ON") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED03, (byte)LEDs_State.ON, 0x00, 0x00);
            if (cmb_LED3.Text == "OFF") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED03, (byte)LEDs_State.OFF, 0x00, 0x00);
        }

        private void cmb_LED4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED4.Text == "ON") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED04, (byte)LEDs_State.ON, 0x00, 0x00);
            if (cmb_LED4.Text == "OFF") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED04, (byte)LEDs_State.OFF, 0x00, 0x00);
        }

        private void cmb_LED5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED5.Text == "ON") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED05, (byte)LEDs_State.ON, 0x00, 0x00);
            if (cmb_LED5.Text == "OFF") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED05, (byte)LEDs_State.OFF, 0x00, 0x00);
        }

        private void cmb_LED6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED6.Text == "ON")
                Set_param(Toolname.LED, (byte)LEDs_ID.LED06, (byte)LEDs_State.ON, 0x00, 0x00);
            if (cmb_LED6.Text == "OFF") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED06, (byte)LEDs_State.OFF, 0x00, 0x00);
        }

        private void cmb_LED7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED7.Text == "ON")
                Set_param(Toolname.LED, (byte)LEDs_ID.LED07, (byte)LEDs_State.ON, 0x00, 0x00);
            if (cmb_LED7.Text == "OFF") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED07, (byte)LEDs_State.OFF, 0x00, 0x00);
        }

        private void cmb_LED8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_LED8.Text == "ON") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED08, (byte)LEDs_State.ON, 0x00, 0x00);
            if (cmb_LED8.Text == "OFF") 
                Set_param(Toolname.LED, (byte)LEDs_ID.LED08, (byte)LEDs_State.OFF, 0x00, 0x00);
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
                    Update_LCD(Toolname.LCD, backlight, 0x00, 0x00, empty);
                }
            }
        }

        private void txt_lcd2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == (char)Keys.Back) || (e.KeyChar == (char)Keys.Delete))
            {
                if (txt_lcd1.Text == "")
                {
                    Update_LCD(Toolname.LCD, backlight, 0x00, 0x01, empty);
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
                Update_LCD(Toolname.LCD, 0x01, 0x00, 0x03, empty);
            }
            else
            {
                backlight = 0;
                txt_lcd1.BackColor = Color.DarkOliveGreen;
                txt_lcd2.BackColor = Color.DarkOliveGreen;
                Update_LCD(Toolname.LCD, 0x00, 0x00, 0x03, empty);
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

                for (int cnt = 0; cnt < charArray.Length; cnt++)
                {
                    try
                    {
                        _tmp_Splitedstring[cnt] = Convert.ToByte(charArray[cnt]);
                    }
                    catch
                    {
                        _tmp_Splitedstring[cnt] = (byte)' ';
                    }
                }
            }
            Update_LCD(Toolname.LCD, backlight, 0x00, 0x00, _tmp_Splitedstring);
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

                for (int cnt = 0; cnt < charArray.Length; cnt++)
                {
                    try
                    { 
                        _tmp_Splitedstring[cnt] = Convert.ToByte(charArray[cnt]);
                    }
                    catch
                    {
                        _tmp_Splitedstring[cnt] = (byte)' ';
                    }
                }
            }
            Update_LCD(Toolname.LCD, backlight, 0x00, 0x01, _tmp_Splitedstring);
        }

        #endregion

        #region Servo 

        private void num_S1Angle_ValueChanged(object sender, EventArgs e)
        {
            if (SD_Com_en && S_en[0])
            {
                RotateImage(SHorn1, image, (byte)num_S1Angle.Value - 90);
                Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO01, (byte)num_S1Angle.Value, (byte)tb_SSpeed1.Value, (byte)(num_S1S.Value * 10));
            }
        }

        private void num_S2Angle_ValueChanged(object sender, EventArgs e)
        {
            if (SD_Com_en && S_en[1])
            {
                    RotateImage(SHorn2, image, (byte)num_S2Angle.Value - 90);
                    Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO02, (byte)num_S2Angle.Value, (byte)tb_SSpeed2.Value, (byte)(num_S2S.Value * 10));
            }
        }

        private void num_S3Angle_ValueChanged(object sender, EventArgs e)
        {
            if (SD_Com_en && S_en[2])
            {
                RotateImage(SHorn3, image, (byte)num_S3Angle.Value - 90);
                Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO03, (byte)num_S3Angle.Value, (byte)tb_SSpeed3.Value, (byte)(num_S3S.Value * 10));
            }
        }

        private void num_S4Angle_ValueChanged(object sender, EventArgs e)
        {
            if (SD_Com_en && S_en[3])
            {
                RotateImage(SHorn4, image, (byte)num_S4Angle.Value - 90);
                Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO04, (byte)num_S4Angle.Value, (byte)tb_SSpeed4.Value, (byte)(num_S4S.Value * 10));
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
                    SHorn1.Enabled = false;
                    num_S1Angle.Enabled = false;
                    tb_SSpeed1.Enabled = false;
                    num_S1S.Enabled = false;
                    Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO01, (byte)SERVO_COMMAND.OFF, 0x00, 0x00);
                }
                else
                {
                    S_en[0] = true;
                    btn_S1en.Text = "ON";
                    btn_S1en.BackColor = Color.Green;
                    SHorn1.Enabled = true;
                    num_S1Angle.Enabled = true;
                    tb_SSpeed1.Enabled = true;
                    num_S1S.Enabled = true;
                    Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO01, (byte)num_S1Angle.Value, (byte)tb_SSpeed1.Value, (byte)(num_S1S.Value * 10));
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
                    SHorn2.Enabled = false;
                    num_S2Angle.Enabled = false;
                    tb_SSpeed2.Enabled = false;
                    num_S2S.Enabled = false;
                    Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO02, (byte)SERVO_COMMAND.OFF, 0x00, 0x00);
                }
                else
                {
                    S_en[1] = true;
                    btn_S2en.Text = "ON";
                    btn_S2en.BackColor = Color.Green;
                    SHorn2.Enabled = true;
                    num_S2Angle.Enabled = true;
                    tb_SSpeed2.Enabled = true;
                    num_S2S.Enabled = true;
                    Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO02, (byte)num_S2Angle.Value, (byte)tb_SSpeed2.Value, (byte)(num_S2S.Value * 10));
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
                    SHorn3.Enabled = false;
                    num_S3Angle.Enabled = false;
                    tb_SSpeed3.Enabled = false;
                    num_S3S.Enabled = false;
                    Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO03, (byte)SERVO_COMMAND.OFF, 0x00, 0x00);
                }
                else
                {
                    S_en[2] = true;
                    btn_S3en.Text = "ON";
                    btn_S3en.BackColor = Color.Green;
                    SHorn3.Enabled = true;
                    num_S3Angle.Enabled = true;
                    tb_SSpeed3.Enabled = true;
                    num_S3S.Enabled = true;
                    Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO03, (byte)num_S3Angle.Value, (byte)tb_SSpeed3.Value, (byte)(num_S3S.Value * 10));
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
                    SHorn4.Enabled = false;
                    num_S4Angle.Enabled = false;
                    tb_SSpeed4.Enabled = false;
                    num_S4S.Enabled = false;
                    Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO04, (byte)SERVO_COMMAND.OFF, 0x00, 0x00);
                }
                else
                {
                    S_en[3] = true;
                    btn_S4en.Text = "ON";
                    btn_S4en.BackColor = Color.Green;
                    SHorn4.Enabled = true;
                    num_S4Angle.Enabled = true;
                    tb_SSpeed4.Enabled = true;
                    num_S4S.Enabled = true;
                    Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO04, (byte)num_S4Angle.Value, (byte)tb_SSpeed4.Value, (byte)(num_S4S.Value * 10));
                }
            }
        }

        private void tb_SSpeed1_MouseUp(object sender, MouseEventArgs e)
        {
            if( SD_Com_en && S_en[0] )
                Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO01, (byte)num_S1Angle.Value, (byte)tb_SSpeed1.Value, (byte)(num_S1S.Value * 10));
        }

        private void tb_SSpeed2_MouseUp(object sender, MouseEventArgs e)
        {
            if (SD_Com_en && S_en[1])
                Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO02, (byte)num_S2Angle.Value, (byte)tb_SSpeed2.Value, (byte)(num_S2S.Value * 10));
        }

        private void tb_SSpeed3_MouseUp(object sender, MouseEventArgs e)
        {
            if (SD_Com_en && S_en[2])
                Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO03, (byte)num_S3Angle.Value, (byte)tb_SSpeed3.Value, (byte)(num_S3S.Value * 10));
        }

        private void tb_SSpeed4_MouseUp(object sender, MouseEventArgs e)
        {
            if (SD_Com_en && S_en[3])
                Set_param(Toolname.SERVO_MOTOR, (byte)SERVOS_ID.SERVO04, (byte)num_S4Angle.Value, (byte)tb_SSpeed4.Value, (byte)(num_S4S.Value * 10));
        }


        private void SHorn1_MouseUp(object sender, MouseEventArgs e)
        {
            if ( SD_Com_en && S_en[0])
            {
                float tempDegree = 0;

                if (e != null)
                {
                    float xCoordinate = e.X;
                    float yCoordinate = e.Y;
                    PointF MPU = new PointF(-(xCoordinate - 45), -(yCoordinate - 39));
                    tempDegree = (int)Polar_Cordinate_Extender(MPU);
                }
                else
                {
                    tempDegree = float.Parse(num_S1Angle.Text);
                }

                if (tempDegree < 0)
                    tempDegree += 360;

                if ((Math.Abs(tempDegree) > 180) && (Math.Abs(tempDegree) <= 270)) 
                    tempDegree = 180;

                if ((Math.Abs(tempDegree) >= 270)) 
                    tempDegree = 0;

                num_S1Angle.Value = Convert.ToDecimal( tempDegree );
            }
            else
            {
                MessageBox.Show(this, "Please turn ON the Servo", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SHorn2_MouseUp(object sender, MouseEventArgs e)
        {
            if (SD_Com_en && S_en[1])
            {
                float tempDegree = 0;

                if (e != null)
                {
                    float xCoordinate = e.X;
                    float yCoordinate = e.Y;
                    PointF MPU = new PointF(-(xCoordinate - 45), -(yCoordinate - 39));
                    tempDegree = (int)Polar_Cordinate_Extender(MPU);
                }
                else
                {
                    tempDegree = float.Parse(num_S2Angle.Text);
                }

                if (tempDegree < 0)
                    tempDegree += 360;

                if ((Math.Abs(tempDegree) > 180) && (Math.Abs(tempDegree) <= 270))
                    tempDegree = 180;

                if ((Math.Abs(tempDegree) >= 270))
                    tempDegree = 0;

                num_S2Angle.Value = Convert.ToDecimal(tempDegree);
            }
            else
            {
                MessageBox.Show(this, "Please turn ON the Servo", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SHorn3_MouseUp(object sender, MouseEventArgs e)
        {
            if (SD_Com_en && S_en[2])
            {
                float tempDegree = 0;

                if (e != null)
                {
                    float xCoordinate = e.X;
                    float yCoordinate = e.Y;
                    PointF MPU = new PointF(-(xCoordinate - 45), -(yCoordinate - 39));
                    tempDegree = (int)Polar_Cordinate_Extender(MPU);
                }
                else
                {
                    tempDegree = float.Parse(num_S3Angle.Text);
                }

                if (tempDegree < 0) 
                    tempDegree += 360;

                if ((Math.Abs(tempDegree) > 180) && (Math.Abs(tempDegree) <= 270)) 
                    tempDegree = 180;

                if ((Math.Abs(tempDegree) >= 270)) 
                    tempDegree = 0;

                num_S3Angle.Value = Convert.ToDecimal(tempDegree);
            }
            else
            {
                MessageBox.Show(this, "Please turn ON the Servo", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SHorn4_MouseUp(object sender, MouseEventArgs e)
        {
            if (SD_Com_en && S_en[3])
            {
                float tempDegree = 0;

                if (e != null)
                {
                    float xCoordinate = e.X;
                    float yCoordinate = e.Y;
                    PointF MPU = new PointF(-(xCoordinate - 45), -(yCoordinate - 39));
                    tempDegree = (int)Polar_Cordinate_Extender(MPU);
                }
                else
                {
                    tempDegree = float.Parse(num_S4Angle.Text);
                }

                if (tempDegree < 0)
                    tempDegree += 360;

                if ((Math.Abs(tempDegree) > 180) && (Math.Abs(tempDegree) <= 270)) 
                    tempDegree = 180;

                if ((Math.Abs(tempDegree) >= 270)) 
                    tempDegree = 0;

                num_S4Angle.Value = Convert.ToDecimal(tempDegree);
            }
            else
            {
                MessageBox.Show(this, "Please turn ON the Servo", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SyncSendS_Click(object sender, EventArgs e)
        {
            decimal tempAngle = num_SyncAS.Value;
            int tempSpeed = (int)num_SyncSS.Value;
            decimal tempDuration = num_SyncDS.Value;

            if (S_en[0])
            {
                tb_SSpeed1.Value = tempSpeed;
                num_S1S.Value = tempDuration;
                num_S1Angle.Value = tempAngle;
            }
            if (S_en[1])
            {
                tb_SSpeed2.Value = tempSpeed;
                num_S2S.Value = tempDuration;
                num_S2Angle.Value = tempAngle;
            }
            if (S_en[2])
            {
                tb_SSpeed3.Value = tempSpeed;
                num_S3S.Value = tempDuration;
                num_S3Angle.Value = tempAngle;
            }
            if (S_en[3])
            {
                tb_SSpeed4.Value = tempSpeed;
                num_S4S.Value = tempDuration;
                num_S4Angle.Value = tempAngle;
            }
        }

        private void btn_SuncStopS_Click(object sender, EventArgs e)
        {
            num_SyncAS.Value = 90;

            if (S_en[0])
            {
                num_S1Angle.Value = 90;
            }
            if (S_en[1])
            {
                num_S2Angle.Value = 90;
            }
            if (S_en[2])
            {
                num_S3Angle.Value = 90;
            }
            if (S_en[3])
            {
                num_S4Angle.Value = 90;
            }
        }

        private void tb_SSpeed1_ValueChanged(object sender, EventArgs e)
        {
            lbl_SS1.Text = (tb_SSpeed1.Value).ToString();
        }

        private void tb_SSpeed2_ValueChanged(object sender, EventArgs e)
        {
            lbl_SS2.Text = (tb_SSpeed2.Value).ToString();
        }

        private void tb_SSpeed3_ValueChanged(object sender, EventArgs e)
        {
            lbl_SS3.Text = (tb_SSpeed3.Value).ToString();
        }

        private void tb_SSpeed4_ValueChanged(object sender, EventArgs e)
        {
            lbl_SS4.Text = (tb_SSpeed4.Value).ToString();
        }
        #endregion

        #endregion

        //***********************************************************************************

        #region Funtions

        /// <summary>
        /// Rotate image 
        /// </summary>
        /// <param name="pb">Proposed PictureBox</param>
        /// <param name="img">Proposed Image that you want to rotate it</param>
        /// <param name="angle">Desire angle to rotation</param>
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
        /// At the first of connection with Auto-search selection .. this function ping the PRC without the com selection
        /// </summary>
        /// <returns>this function if the Ping progress was successful returns true otherwise false</returns>
        private bool Ping()
        {
            byte[] _data = new byte[10];
            byte _inp = 0;
            bool _status = false;

            SD_Com.DtrEnable = true;

            try
            {
                _inp = (byte)SD_Com.ReadByte();
                if (_inp == 0x39)
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
                _status = false;
            }
            return _status;
        }
        //***********************************************************************************
        /// <summary>
        /// this function handles the all situation about com-port and primary connection
        /// </summary>
        private void _Start()
        {
            string tempSelection = toolStripComboBox1.SelectedItem.ToString();
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("Auto Search");
            toolStripComboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            if (System.IO.Ports.SerialPort.GetPortNames().Any(x => x == tempSelection))
                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf(tempSelection);
            else
                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf("Auto Search");
            //###################################################################

            #region Port OPEN
            if (btn_Connect.ToolTipText == "Connect")
            {
                #region Autosearch
                if (toolStripComboBox1.SelectedIndex == toolStripComboBox1.Items.IndexOf("Auto Search"))
                {
                    string[] systemComPorts = new string[100];

                    systemComPorts = System.IO.Ports.SerialPort.GetPortNames();
                    if (systemComPorts.Count() == 0)
                        MessageBox.Show(this, "Connect PRC to PC!!.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        bool tempPingRes = false;
                        try
                        {
                            for (int cnt = 0; cnt < toolStripComboBox1.Items.Count; cnt++)
                            {
                                SD_Com = new SerialPort(systemComPorts[cnt], 115200);
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
                                            tempPingRes = Ping();
                                            Thread.Sleep(1);
                                            if (tempPingRes)
                                            {
                                                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf(SD_Com.PortName.ToString());
                                                readyForm();
                                                break;
                                            }
                                            else
                                                SD_Com.Close();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.Beep();
                                            MessageBox.Show(this, ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            break;
                                        }
                                    }
                                }
                                Thread.Sleep(1);
                                if (tempPingRes) break;
                            }
                        }
                        catch
                        {
                            SD_Com.Dispose();
                            Console.Beep();
                            MessageBox.Show(this, "Check Connection!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                #endregion

                #region Manual
                else
                {
                    if (toolStripComboBox1.SelectedIndex == -1)
                        MessageBox.Show(this, "Select Com-port!!.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        SD_Com = new SerialPort(toolStripComboBox1.SelectedItem.ToString(),115200);
                        if (SD_Com.IsOpen == false)
                        {
                            try
                            {
                                SD_Com.ReadTimeout = 100;
                                SD_Com.Open();
                                if( ! Ping() ) throw new Exception("This is not correct Com-Port!");    // exception for incorrect com-port selection
                                readyForm();
                            }
                            catch (Exception ex)
                            {
                                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf("Auto Search");
                                User_des.Write("User_Com", "");
                                SD_Com.Dispose();
                                Console.Beep();
                                MessageBox.Show(this, ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

            //###################################################################

            #region PORT Close
            else
            {
                try
                {
                    Send_close_command();
                }
                catch
                {
                }

                resetForm();
                //Image_R.Clone();
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
                Tx_tmp[0] = HEADER;
                Tx_tmp[1] = HEADER;
                Tx_tmp[2] = (byte)'E';
                Tx_tmp[3] = (byte)'X';

                int checksum = Tx_tmp[2] + Tx_tmp[3];
                Tx_tmp[4] = (byte)checksum;
                Tx_tmp[5] = (byte)(checksum >> 8);

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        SD_Com.Write(Tx_tmp, 0, 6);
                        SD_Com.DiscardOutBuffer();
                        SD_Com.DiscardInBuffer();
                    }
                    catch 
                    {
                        
                    }
                }
                
            }
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tool_Name">One of defined components to connect to PRC</param>
        /// <param name="Tool_ID">The port number proposed to connect the sensor or I/O module</param>
        /// <param name="_param0">special parameter considering the type of connected module</param>
        /// <param name="_param1">special parameter considering the type of connected module</param>
        /// <param name="_param2">special parameter considering the type of connected module</param>
        private void Set_param(Toolname Tool_Name, byte Tool_ID, byte _param0, byte _param1, byte _param2)
        {
            if (SD_Com_en)
            {
                Tx_tmp[0] = HEADER;
                Tx_tmp[1] = HEADER;
                Tx_tmp[2] = (byte)Tool_Name;
                Tx_tmp[3] = Tool_ID;
                Tx_tmp[4] = _param0;   //Speed or State
                Tx_tmp[5] = _param1;   //Direction
                Tx_tmp[6] = _param2;   //Time

                int checksum = (byte)Tool_Name + Tool_ID + _param0 + _param1 + _param2;
                Tx_tmp[7] = (byte)checksum;
                Tx_tmp[8] = (byte)(checksum >> 8);
                //
                for (byte cnt = 0; cnt < 3; cnt++)
                {
                    try
                    {
                        SD_Com.Write(Tx_tmp, 0, 9);
                        SD_Com.DiscardOutBuffer();
                    }
                    catch 
                    {
                        
                    }
                }
                
            }
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Backlight">0x01->Backlight of LCD->ON, 0x00->Backlight of LCD->OFF</param>
        /// <param name="x_pos">Desire X position to Write the characters on LCD</param>
        /// <param name="y_pos">Desire y position to Write the characters on LCD</param>
        /// <param name="character">The spitted String to write and show in LCD in byte 'ASCI' with maximum length=8</param>
        private void Update_LCD(Toolname Tool_Name, byte Backlight, byte x_pos, byte y_pos, byte[] character)
        {
            if (SD_Com_en)
            {
                Tx_tmp[0] = HEADER;
                Tx_tmp[1] = HEADER;
                Tx_tmp[2] = (byte)Tool_Name;
                //Backlight control command
                Tx_tmp[3] = Backlight;
                //X-position in LCD 
                Tx_tmp[4] = x_pos;
                //Y-position in LCD 
                Tx_tmp[5] = y_pos;
                //Character of one line
                for (int i = 0; i < 8; i++ )
                    Tx_tmp[6+i] = character[i];

                int checksum = 0;

                for (byte cnt = 0; cnt <= 7; cnt++) 
                    checksum += character[cnt];

                checksum += (byte)Tool_Name;
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
                        SD_Com.DiscardOutBuffer();
                    }
                    catch 
                    {
                        
                    }
                }
                
            }
        }
        //***********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tool_Name">One of defined components to connect to PRC</param>
        /// <param name="Tool_ID">The port number proposed to connect the sensor or I/O module</param>
        /// <returns>this function returns the special value of desired Input module</returns>
        private void Read_param_request(Toolname Tool_Name, byte Tool_ID)
        {
            if (SD_Com_en)
            {
                int checksum = 0;
                Tx_tmp[0] = HEADER;
                Tx_tmp[1] = HEADER;
                Tx_tmp[2] = (byte)Tool_Name;
                Tx_tmp[3] = Tool_ID;

                checksum = (byte)Tool_Name + Tool_ID;
                Tx_tmp[4] = (byte)checksum;
                Tx_tmp[5] = (byte)(checksum >> 8);

                try
                {
                    SD_Com.Write(Tx_tmp, 0, 6);
                    SD_Com.DiscardOutBuffer();
                }
                    
                catch
                {
                }
            }
            Thread.Sleep(10);
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
        /// 
        /// </summary>
        private void Read_Req_Thread()
        {
            if (SD_Com_en)
            {
                Read_Req_Th = new Thread(new ParameterizedThreadStart(
                    new Action<object>((t) =>
                    {
                        Read_Req_Th_ex = true;
                        while (thread_enable)
                        {
                            Thread.Sleep(40);
                            if (_Sensor_Enable[0] == 1)
                                Read_param_request(_Tool_Name[0], (byte)ADs_ID.AD01);

                            if (_Sensor_Enable[1] == 1)
                                Read_param_request(_Tool_Name[1], (byte)ADs_ID.AD02);

                            if (_Sensor_Enable[2] == 1)
                                Read_param_request(_Tool_Name[2], (byte)ADs_ID.AD03);

                            if (_Sensor_Enable[3] == 1)
                                Read_param_request(_Tool_Name[3], (byte)ADs_ID.AD04);

                            if (_Sensor_Enable[4] == 1)
                                Read_param_request(_Tool_Name[4], (byte)ADs_ID.AD05);

                            if (_Sensor_Enable[5] == 1)
                                Read_param_request(_Tool_Name[5], (byte)ADs_ID.AD06);

                            if (_Sensor_Enable[6] == 1)
                                Read_param_request(_Tool_Name[6], (byte)ADs_ID.AD07);

                            if (_Sensor_Enable[7] == 1)
                                Read_param_request(_Tool_Name[7], (byte)ADs_ID.AD08);
                        }
                    })));

                Read_Req_Th.SetApartmentState(ApartmentState.STA);
                Read_Req_Th.Start();
            }
            else
            {
                MessageBox.Show(this,"Please Open Com-port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Read_Thread()
        {
            if (SD_Com_en)
            {
                Read_Th = new Thread(new ParameterizedThreadStart(
                    new Action<object>((t) =>
                    {
                        Read_Th_ex = true;
                        while ( thread_enable )
                        {
                            Thread.Sleep(1);
                            try
                            {
                                while (SD_Com.BytesToRead > 0)
                                {
                                    try
                                    {
                                        SD_Com.ReadTimeout = 10;
                                        if ((byte)SD_Com.ReadByte() == HEADER)
                                        {
                                            if ((byte)SD_Com.ReadByte() == HEADER)
                                            {
                                                Toolname Tool_Name = (Toolname)SD_Com.ReadByte();
                                                if (Tool_Name == Toolname.IR_SENSOR ||
                                                    Tool_Name == Toolname.LIGHT_SENSOR ||
                                                    Tool_Name == Toolname.SOUND_SENSOR ||
                                                    Tool_Name == Toolname.TOUCH_SENSOR)
                                                {
                                                    byte Tool_ID = (byte)SD_Com.ReadByte();
                                                    if (Tool_ID >= 0x01 && Tool_ID <= 0x08)
                                                    {
                                                        short Tool_Data = (byte)SD_Com.ReadByte();
                                                        short[] Check_Byte = new Int16[2];
                                                        Check_Byte[0] = (byte)SD_Com.ReadByte();
                                                        Check_Byte[1] = (byte)SD_Com.ReadByte();

                                                        int checksum = (byte)Tool_Name + Tool_ID + Tool_Data;
                                                        int _checksum = (Check_Byte[1] << 8) + Check_Byte[0];

                                                        if (_checksum == checksum)
                                                        {
                                                            if (_Tool_Name[Tool_ID - 1] == Tool_Name)
                                                            {
                                                                res_tSen[Tool_ID - 1] = Tool_Data;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    })));

                Read_Th.SetApartmentState(ApartmentState.STA);
                Read_Th.Start();
            }
            else
            {
                MessageBox.Show(this, "Please Open Com-port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //***********************************************************************************
        /// <summary>
        /// This function Rotates an image in a specific pictureBox through EMGUCVs methods
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
        /// this function extracts the polar position from scalar coordinates 
        /// </summary>
        /// <param name="Mouse_Pos"></param>
        private float Polar_Cordinate_Extender(PointF Mouse_Pos)
        {
            float _angle = 0;
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
            Set_param(Toolname.BUZZER, (byte)num_BRC.Value, (byte)(num_BONt.Value * 10), (byte)(num_BOFFt.Value * 10), 0x00);
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
                    Thread.Sleep(10);
                    if (SD_Com_en)  Send_close_command();
                    //Image_R.Clone();

                    if (Read_Req_Th_ex || Read_Th_ex || SD_Com_en)
                    {
                        Thread.Sleep(10); 
                        Read_Req_Th.Abort();
                        Read_Th.Abort();
                        SD_Com.Dispose();
                    }
                    Thread.Sleep(10);
                    this.Dispose(true);
                    Thread.Sleep(10);
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
                    Thread.Sleep(40);
                    if (Send_first[0] == 1)
                    {
                        _tmp_angle[0] += _Motor_Speed[0] / 6;
                        RotateImage(pb_W1, (Bitmap)Properties.Resources.pully, _tmp_angle[0]);
                    }
                    if (Send_first[1] == 1)
                    {
                        _tmp_angle[1] += _Motor_Speed[1] / 6;
                        RotateImage(pb_W2, (Bitmap)Properties.Resources.pully, _tmp_angle[1]);
                    }
                    if (Send_first[2] == 1)
                    {
                        _tmp_angle[2] += _Motor_Speed[2] / 6;
                        RotateImage(pb_W3, (Bitmap)Properties.Resources.pully, _tmp_angle[2]);
                    }
                    if (Send_first[3] == 1)
                    {
                        _tmp_angle[3] += _Motor_Speed[3] / 6;
                        RotateImage(pb_W4, (Bitmap)Properties.Resources.pully, _tmp_angle[3]);
                    }
                }
            });
            Image_R.BeginInvoke(null, null);
        }

        public static void ThreadProc()
        {
            Application.Run(new Rrealtime_Debugger());
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

        private void pb_LED10_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                if (LED_S[0]==0)
                {
                    pb_LED10.Image = Properties.Resources.Untitled;
                    Set_param(Toolname.LED, (byte)LEDs_ID.LED10, (byte)LEDs_State.ON, 0x00, 0x00);
                    LED_S[0] = 1;
                }
                else
                {
                    pb_LED10.Image = Properties.Resources._1;
                    Set_param(Toolname.LED, (byte)LEDs_ID.LED10, (byte)LEDs_State.OFF, 0x00, 0x00);
                    LED_S[0] = 0;
                }
            }
        }

        private void pb_LED9_Click(object sender, EventArgs e)
        {
            if (SD_Com_en)
            {
                if (LED_S[1] == 0)
                {
                    pb_LED9.Image = Properties.Resources.Untitled;
                    Set_param(Toolname.LED, (byte)LEDs_ID.LED09, (byte)LEDs_State.ON, 0x00, 0x00);
                    LED_S[1] = 1;
                }
                else
                {
                    pb_LED9.Image = Properties.Resources._1;
                    Set_param(Toolname.LED, (byte)LEDs_ID.LED09, (byte)LEDs_State.OFF, 0x00, 0x00);
                    LED_S[1] = 0;
                }
            }
        }

        private void connection_timer_Tick(object sender, EventArgs e)
        {
            if ( ! isPortAvailable() )
            {
                connection_timer.Enabled = false;
                resetForm();
                Console.Beep();
                MessageBox.Show(this, "Port is closed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private bool isPortAvailable( )
        {
            try
            {
                return System.IO.Ports.SerialPort.GetPortNames().Any(x => x == SD_Com.PortName);
            }
            catch
            {
                return false;
            }
        }
        //***********************************************************************************
        private void resetForm( )
        {
            if (Read_Req_Th_ex || Read_Th_ex || SD_Com.IsOpen)
            {
                try
                {
                    Read_Req_Th.Abort();
                    Read_Th.Abort();
                    SD_Com.Dispose();
                }
                catch
                {

                }
            }

            #region Global Variables
            SD_Com_en = false;
            Read_Req_Th_ex = false;
            Read_Th_ex = false;
            #endregion

            #region DC Motors

            #region All DC Motors
            cmb_SyncDM.SelectedIndex = -1;  // set default direction
            num_SyncSM.Value = 0;           // set default speed
            num_SyncTM.Value = 0;           // set default duration
            SyncTimer.Enabled = false;      // turn off sync timer
            groupBox9.Enabled = false;      // disable All DC Motors group
            #endregion

            for (int i = 0; i < 4; i++)     // turn off animation
            {
                _tmp_angle[i] = 0;
                _Motor_Speed[i] = 0;
                Send_first[i] = 0;
            }

            #region DC Motor 01
            RotateImage(pb_W1, (Bitmap)Properties.Resources.pully, 0);  // rotate pulley to default position
            trb_SM1.Value = 0;              // set default speed
            num_TimeM1.Value = 0;           // set default duration
            TM1.Enabled = false;            // turn off DC Motor 1 Timer
            groupBox3.Enabled = false;      // disable DC Motor 1 group
            #endregion

            #region DC Motor 02
            RotateImage(pb_W2, (Bitmap)Properties.Resources.pully, 0);  // rotate pulley to default position
            trb_SM2.Value = 0;              // set default speed
            num_TimeM2.Value = 0;           // set default duration
            TM2.Enabled = false;            // turn off DC Motor 2 Timer
            groupBox2.Enabled = false;
            #endregion

            #region DC Motor 03
            RotateImage(pb_W3, (Bitmap)Properties.Resources.pully, 0);  // rotate pulley to default position
            trb_SM3.Value = 0;              // set default speed
            num_TimeM3.Value = 0;           // set default duration
            TM3.Enabled = false;            // turn off DC Motor 3 Timer
            groupBox4.Enabled = false;
            #endregion

            #region DC Motor 04
            RotateImage(pb_W4, (Bitmap)Properties.Resources.pully, 0);  // rotate pulley to default position
            trb_SM4.Value = 0;              // set default speed
            num_TimeM4.Value = 0;           // set default duration
            TM4.Enabled = false;            // turn off DC Motor 4 Timer
            groupBox5.Enabled = false;
            #endregion

            #endregion

            #region Servo Motors

            #region All Servo Motors
            num_SyncAS.Value = 90;
            num_SyncSS.Value = 250;
            num_SyncDS.Value = 0;
            groupBox10.Enabled = false;
            #endregion

            #region Servo 01
            RotateImage(SHorn1, image, 0);  // rotate horn to default position
            SHorn1.Enabled = false;         // disable horn picture
            num_S1Angle.Value = 90;         // set default angle
            num_S1Angle.Enabled = false;    // disable angle box
            tb_SSpeed1.Value = 250;         // set default speed
            tb_SSpeed1.Enabled = false;     // disable speed handle
            num_S1S.Value = 0;              // set default duration
            num_S1S.Enabled = false;        // disable duration box
            btn_S1en.BackColor = System.Drawing.Color.OrangeRed;    // set default button color
            btn_S1en.Text = "OFF";          // set default button text
            #endregion

            #region Servo 02
            RotateImage(SHorn2, image, 0);  // rotate horn to default position
            SHorn2.Enabled = false;         // disable horn picture
            num_S2Angle.Value = 90;         // set default angle
            num_S2Angle.Enabled = false;    // disable angle box
            tb_SSpeed2.Value = 250;         // set default speed
            tb_SSpeed2.Enabled = false;     // disable speed handle
            num_S2S.Value = 0;              // set default duration
            num_S2S.Enabled = false;        // disable duration box
            btn_S2en.BackColor = System.Drawing.Color.OrangeRed;    // set default button color
            btn_S2en.Text = "OFF";          // set default button text
            #endregion

            #region Servo 03
            RotateImage(SHorn3, image, 0);  // rotate horn to default position
            SHorn3.Enabled = false;         // disable horn picture
            num_S3Angle.Value = 90;         // set default angle
            num_S3Angle.Enabled = false;    // disable angle box
            tb_SSpeed3.Value = 250;         // set default speed
            tb_SSpeed3.Enabled = false;     // disable speed handle
            num_S3S.Value = 0;              // set default duration
            num_S3S.Enabled = false;        // disable duration box
            btn_S3en.BackColor = System.Drawing.Color.OrangeRed;    // set default button color
            btn_S3en.Text = "OFF";          // set default button text
            #endregion

            #region Servo 04
            RotateImage(SHorn4, image, 0);  // rotate horn to default position
            SHorn4.Enabled = false;         // disable horn picture
            num_S4Angle.Value = 90;         // set default angle
            num_S4Angle.Enabled = false;    // disable angle box
            tb_SSpeed4.Value = 250;         // set default speed
            tb_SSpeed4.Enabled = false;     // disable speed handle
            num_S4S.Value = 0;              // set default duration
            num_S4S.Enabled = false;        // disable duration box
            btn_S4en.BackColor = System.Drawing.Color.OrangeRed;    // set default button color
            btn_S4en.Text = "OFF";          // set default button text
            #endregion

            groupBox6.Enabled = false;
            groupBox7.Enabled = false;

            for( int i = 0; i < 4; i++ )
                S_en[i] = false;
            #endregion

            #region AD & LED Ports

            #region All AD Ports
            cmb_Allsensors.SelectedIndex = -1;
            groupBox11.Enabled = false;
            #endregion

            #region AD Port 01
            com_ADC1.SelectedIndex = 0;
            lbl_ADC1.Text = "0.0";
            #endregion

            #region AD Port 02
            com_ADC2.SelectedIndex = 0;
            lbl_ADC2.Text = "0.0";
            #endregion

            #region AD Port 03
            com_ADC3.SelectedIndex = 0;
            lbl_ADC3.Text = "0.0";
            #endregion

            #region AD Port 04
            com_ADC4.SelectedIndex = 0;
            lbl_ADC4.Text = "0.0";
            #endregion

            #region AD Port 05
            com_ADC5.SelectedIndex = 0;
            lbl_ADC5.Text = "0.0";
            #endregion

            #region AD Port 06
            com_ADC6.SelectedIndex = 0;
            lbl_ADC6.Text = "0.0";
            #endregion

            #region AD Port 07
            com_ADC7.SelectedIndex = 0;
            lbl_ADC7.Text = "0.0";
            #endregion

            #region AD Port 08
            com_ADC8.SelectedIndex = 0;
            lbl_ADC8.Text = "0.0";
            #endregion

            Sensors.Enabled = false;
            groupBox1.Enabled = false;

            for (int i = 0; i < 8; i++)
                res_tSen[i] = 0;

            #region LED 09
            pb_LED9.Image = Properties.Resources._1;
            LED_S[1] = 0;
            #endregion

            #region LED 10
            pb_LED10.Image = Properties.Resources._1;
            LED_S[0] = 0;
            #endregion
            
            #endregion

            #region LCD
            #region Back Light
            backlight = 1;
            chb_Backlight.CheckState = System.Windows.Forms.CheckState.Checked;
            chb_Backlight.Enabled = false;
            #endregion

            #region LCD Line 01
            txt_lcd1.Text = "  REAL";
            txt_lcd1.Enabled = false;
            #endregion

            #region LCD Line 02
            txt_lcd2.Text = "  TIME";
            txt_lcd2.Enabled = false;
            #endregion
            #endregion

            #region Beeper
            num_BRC.Value = 1;
            num_BONt.Value = 0;
            num_BOFFt.Value = 0;
            groupBox12.Enabled = false;
            #endregion

            #region Tool Strip

            #region connection button
            btn_Connect.Image = Properties.Resources.Connect;
            btn_Connect.ToolTipText = "Connect";
            #endregion

            #region COM-Ports Combo
            toolStripComboBox1.Enabled = true;

            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("Auto Search");
            toolStripComboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            if (System.IO.Ports.SerialPort.GetPortNames().Any(x => x == User_des.Read("User_Com")))
                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf(User_des.Read("User_Com"));
            else
                toolStripComboBox1.SelectedIndex = toolStripComboBox1.Items.IndexOf("Auto Search");
            #endregion

            #endregion

            connection_timer.Enabled = false;

        }
        #endregion

        private void readyForm()
        {
            Console.Beep();
            User_des.Write("User_Com", SD_Com.PortName.ToString());
            btn_Connect.ToolTipText = "Disconnect";
            btn_Connect.Image = Properties.Resources.Disconnect;
            toolStripComboBox1.Enabled = false;
            SD_Com_en = true;
            Read_Req_Thread();
            Read_Thread();
            connection_timer.Enabled = true;
            #region Enable Form Controls
            Sensors.Enabled = true;
            groupBox1.Enabled = true;
            groupBox2.Enabled = true;
            groupBox3.Enabled = true;
            groupBox4.Enabled = true;
            groupBox5.Enabled = true;
            groupBox6.Enabled = true;
            groupBox7.Enabled = true;
            groupBox9.Enabled = true;
            groupBox10.Enabled = true;
            groupBox11.Enabled = true;
            groupBox12.Enabled = true;
            chb_Backlight.Enabled = true;
            txt_lcd1.Enabled = true;
            txt_lcd2.Enabled = true;
            #endregion
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.pishrobot.com");
        }
        
    }
}
