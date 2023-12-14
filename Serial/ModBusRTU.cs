using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace GeneralUtility.Serial
{
    public class ModBusRTU
    {
        private SerialPort _clsSerialPort = new SerialPort();
        public string strModbusStatus;


        #region "Init"

        public ModBusRTU()
        {
        }
        ~ModBusRTU()
        {
            Close();
        }

        #endregion

        public bool Open(string strPort, int iBaudRate, int iDatabits, Parity eParity, StopBits eStopBits)
        {
            if (!_clsSerialPort.IsOpen)
            {
                _clsSerialPort.PortName = strPort;
                _clsSerialPort.BaudRate = iBaudRate;
                _clsSerialPort.DataBits = iDatabits;
                _clsSerialPort.Parity = eParity;
                _clsSerialPort.StopBits = eStopBits;
                _clsSerialPort.ReadTimeout = 1000;
                _clsSerialPort.WriteTimeout = 1000;

                try
                {
                    _clsSerialPort.Open();
                }
                catch (Exception e)
                {
                    strModbusStatus = "Error Opening " + strPort + ": " + e.Message;
                    return false;
                }
                strModbusStatus = strPort + " Opened Successfully";
                return true;
            }
            else
            {
                strModbusStatus = strPort + " Already Opened";
                return false;
            }
        }

        public bool Close()
        {
            if (_clsSerialPort.IsOpen)
            {
                try
                {
                    _clsSerialPort.Close();
                }
                catch (Exception err)
                {
                    strModbusStatus = "Error Closing " + _clsSerialPort.PortName + ": " + err.Message;
                    return false;
                }
                strModbusStatus = _clsSerialPort.PortName + " Closed Successfully";
                return true;
            }
            else
            {
                strModbusStatus = _clsSerialPort.PortName + " is not open";
                return false;
            }
        }

        private void GetCRC16(byte[] bMessages, ref byte[] CRC)
        {
            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;

            for (int i = 0; i < (bMessages.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ bMessages[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
        }

        private void BuildMessage(byte bAddress, byte bType, ushort iStart, ushort iRegisters, ref byte[] bMessage)
        {
            //Array to receive CRC bytes:
            byte[] CRC = new byte[2];

            bMessage[0] = bAddress;
            bMessage[1] = bType;
            bMessage[2] = (byte)(iStart >> 8);
            bMessage[3] = (byte)iStart;
            bMessage[4] = (byte)(iRegisters >> 8);
            bMessage[5] = (byte)iRegisters;

            GetCRC16(bMessage, ref CRC);
            bMessage[bMessage.Length - 2] = CRC[0];
            bMessage[bMessage.Length - 1] = CRC[1];
        }

        private bool CheckCRC16(byte[] bResponse)
        {
            byte[] CRC = new byte[2];
            GetCRC16(bResponse, ref CRC);
            if (CRC[0] == bResponse[bResponse.Length - 2] && CRC[1] == bResponse[bResponse.Length - 1])
                return true;
            else
                return false;
        }

        private void GetResponse(ref byte[] bResponse)
        {
            for (int i = 0; i < bResponse.Length; i++)
            {
                bResponse[i] = (byte)(_clsSerialPort.ReadByte());
            }
        }

        public bool SendFc16(byte address, ushort start, ushort registers, short[] value)
        {
            if (_clsSerialPort.IsOpen)
            {
                _clsSerialPort.DiscardOutBuffer();
                _clsSerialPort.DiscardInBuffer();
                byte[] message = new byte[9 + 2 * registers];
                byte[] response = new byte[8];

                message[6] = (byte)(registers * 2);
                for (int i = 0; i < registers; i++)
                {
                    message[7 + 2 * i] = (byte)(value[i] >> 8);
                    message[8 + 2 * i] = (byte)(value[i]);
                }
                BuildMessage(address, (byte)16, start, registers, ref message);
                
                try
                {
                    _clsSerialPort.Write(message, 0, message.Length);
                    GetResponse(ref response);
                }
                catch (Exception err)
                {
                    strModbusStatus = "Error in Write Event: " + err.Message;
                    return false;
                }
                if (CheckCRC16(response))
                {
                    strModbusStatus = "Write Successful";
                    return true;
                }
                else
                {
                    strModbusStatus = "CRC Error";
                    return false;
                }
            }
            else
            {
                strModbusStatus = "Serial port not open";
                return false;
            }
        }

        public short[] SendFc3(byte address, ushort start, ushort registers)
        {
            if (_clsSerialPort.IsOpen)
            {
                short[] iValues = new short[registers];

                _clsSerialPort.DiscardOutBuffer();
                _clsSerialPort.DiscardInBuffer();

                byte[] message = new byte[8];

                byte[] response = new byte[5 + 2 * registers];

                BuildMessage(address, (byte)3, start, registers, ref message);

                try
                {
                    _clsSerialPort.Write(message, 0, message.Length);
                    GetResponse(ref response);
                }
                catch (Exception err)
                {
                    strModbusStatus = "Error in read event: " + err.Message;
                    return null;
                }

                if (CheckCRC16(response))
                {
                    for (int i = 0; i < (response.Length - 5) / 2; i++)
                    {
                        iValues[i] = response[2 * i + 3];
                        iValues[i] <<= 8;
                        iValues[i] += response[2 * i + 4];
                    }
                    strModbusStatus = "Read successful";
                    return iValues;
                }
                else
                {
                    strModbusStatus = "CRC error";
                    return null;
                }
            }
            else
            {
                strModbusStatus = "Serial port not open";
                return null;
            }

        }
    }
}
