using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore {
    public class RecvBuffer {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize) {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        public ArraySegment<byte> ReadSegment {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }
        public ArraySegment<byte> WriteSegment {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        //Recv 받기 전 최대한 여유공간을 갖기 위해 _writePos, _readPos 조정
        public void Clean() {
            int dataSize = DataSize;
            if (dataSize == 0) {
                //데이터를 모두 읽었으면 처음으로
                _writePos = _readPos = 0;
            }
            else {
                //덜 읽은 데이터가 있으면 해당 데이터를 처음으로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        //Recv한 데이터를 읽었고 문제 없음을 나타냄
        public bool OnRead(int numOfBytes) {
            if (numOfBytes > DataSize) {
                return false;
            }

            _readPos += numOfBytes;
            return true;
        }

        //Recv한 데이터의 범위를 표시하고 데이터가 문제없음을 나타냄
        public bool OnWrite(int numOfBytes) {
            if (numOfBytes > FreeSize) {
                return false;
            }

            _writePos += numOfBytes;
            return true;
        }
    }
}
