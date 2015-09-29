using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketMachine.Devices
{
    public class CRC16
    {
        /// <summary>
        /// Расчет CRC16 MODBUS
        /// </summary>
        /// <param name="data">Массив данных</param>
        /// <returns>CRC16 MODBUS</returns>
        public static byte[] ComputeCRC16(byte[] data)
        {
            // MODBUS RTU CRC
            UInt16 crc = 0xFFFF;

            for (int pos = 0; pos < data.Length; pos++)
            {
                crc ^= (UInt16)data[pos];          // XOR byte into least sig. byte of crc

                for (int i = 8; i != 0; i--)
                {    // Loop over each bit
                    if ((crc & 0x0001) != 0)
                    {      // If the LSB is set
                        crc >>= 1;                    // Shift right and XOR 0xA001
                        crc ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        crc >>= 1;                    // Just shift right
                }
            }
            // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)
            return BitConverter.GetBytes(crc);
        }

        /// <summary>
        /// Упаковки данных и СRC в массив
        /// </summary>
        /// <param name="data">Массив данных</param>
        /// <param name="crc">Массив CRC</param>
        /// <returns>Массив data+crc</returns>
        public static byte[] CRC16Pack(byte[] data, byte[] crc)
        {
            byte[] result = new byte[data.Length + crc.Length];
            Array.Copy(data, result, data.Length);
            Array.Copy(crc, 0, result, data.Length, crc.Length);
            return result;
        }

        /// <summary>
        /// Распаковка данных в массив
        /// </summary>
        /// <param name="data">Массив данных с контрольной суммой</param>
        /// <returns>Массив данных без контрольной суммы</returns>
        public static byte[] CRC16UnPack(byte[] data)
        {
            byte[] result = new byte[data.Length - 2];
            Array.Copy(data, result, data.Length - 2);
            return result;
        }
    }
}
