using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace GeneralUtility.IO.XXDevice
{
    public class XX8Relay
    {
        SerialPort _COMPort;

        byte _bHead = 0xE0;
        byte _bEnd = 0x07;

        #region "Init"

        public XX8Relay(string strCOM, int iBaudRate, Parity eParity, int iDataBits, StopBits eStopBits)
        {
            _COMPort = new SerialPort(strCOM, iBaudRate, eParity, iDataBits, eStopBits);
            _COMPort.Open();
        }
        public XX8Relay(string strCOM, int iBaudRate, Parity eParity, int iDataBits)
        {
            _COMPort = new SerialPort(strCOM, iBaudRate, eParity, iDataBits);
            _COMPort.Open();
        }
        public XX8Relay(string strCOM, int iBaudRate, Parity eParity)
        {
            _COMPort = new SerialPort(strCOM, iBaudRate, eParity);
            _COMPort.Open();
        }
        public XX8Relay(string strCOM, int iBaudRates)
        {
            _COMPort = new SerialPort(strCOM, iBaudRates);
            _COMPort.Open();
        }
        public XX8Relay(string strCOM)
        {
            _COMPort = new SerialPort(strCOM);
            _COMPort.Open();
        }

        #endregion

        #region "Function"

        public void SwitchTF(byte iPin, int iDelay)
        {
            SendCommand(0x00, iPin, iDelay);
        }
        public void SwitchFT(byte iPin, int iDelay)
        {
            SendCommand(0x01, iPin, iDelay);
        }
        public void TrunOn(byte iPin)
        {
            SendCommand(0x02, iPin, 100);
        }
        public void TrunOff(byte iPin)
        {
            SendCommand(0x03, iPin, 100);
        }
        public void Flicker(byte iPin, int iDelay)
        {
            SendCommand(0x04, iPin, iDelay);
        }
        public void AllOn()
        {
            SendCommand(0x05, 0, 100);
        }
        public void AllOff()
        {
            SendCommand(0x06, 0, 100);
        }
        public void Reset()
        {
            SendCommand(0x07, 0, 0);
        }
        public void Close()
        {
            if (_COMPort != null)
            {
                if (_COMPort.IsOpen)
                {
                    _COMPort.Close();
                }
            }
        }
        private byte[] SendCommand(byte iType, byte iPin, int iDelay)
        {
            byte[] Commands = new byte[3];
            Commands[2] = (byte)((iPin & 0x0F) << 4);
            Commands[2] = (byte)(Commands[2] | _bEnd);

            if (iDelay > 255)
            {
                if (iDelay > 511) Commands[1] = 0xFF;
                else Commands[1] = (byte)(iDelay & 0x000000FF);

                Commands[0] = (byte)((iType & 0x08) << 1);
                Commands[0] = (byte)(Commands[0] | 0x01);
            }
            else
            {
                Commands[1] = (byte)iDelay;
                Commands[0] = (byte)((iType & 0x07) << 1);
                Commands[0] = (byte)(Commands[0] & 0x0E);
            }
            Commands[0] = (byte)(Commands[0] | _bHead);

            if (_COMPort != null)
            {
                if (_COMPort.IsOpen)
                {
                    _COMPort.Write(Commands, 0, Commands.Length);
                }
            }

            return Commands;
        }

        #endregion
    }
}
