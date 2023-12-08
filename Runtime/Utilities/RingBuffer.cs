/* 
*   VideoKit
*   Copyright © 2023 NatML Inc. All Rights Reserved.
*/

namespace VideoKit.Utilities {

    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    internal sealed class RingBuffer<T> where T : unmanaged {

        #region --Client API--
        /// <summary>
        /// Current buffer length.
        /// This is the number of elements that can be read from the buffer.
        /// </summary>
        public int Length => (write - read + 1);

        /// <summary>
        /// Number of elements that can be written to the buffer.
        /// </summary>
        public int Available => Capacity - Length;

        /// <summary>
        /// Buffer capacity.
        /// </summary>
        public int Capacity => buffer.Length;

        /// <summary>
        /// Create a ring buffer.
        /// </summary>
        /// <param name="capacity">Buffer capacity.</param>
        public RingBuffer (int capacity) {
            this.buffer = new T[capacity];
            Clear();
        }

        /// <summary>
        /// Clear the buffer.
        /// </summary>
        public void Clear () {
            this.read = 0;
            this.write = -1;
            Array.Clear(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Read elements from the buffer.
        /// </summary>
        /// <param name="destination">Destination array.</param>
        public void Read (T[] destination) => Read(destination, 0, destination.Length);

        /// <summary>
        /// Read elements from the buffer.
        /// </summary>
        /// <param name="destination">Destinationa array.</param>
        /// <param name="index">Destination start index.</param>
        /// <param name="length">Number of elements to read.</param>
        public unsafe void Read (T[] destination, int index, int length) {
            fixed (T* baseAddress = destination)
                Read(&baseAddress[index], length);
        }

        /// <summary>
        /// Read elements from the buffer.
        /// </summary>
        /// <param name="destination">Destinationa array.</param>
        public unsafe void Read (NativeArray<T> destination) => Read((T*)destination.GetUnsafeReadOnlyPtr(), destination.Length);

        /// <summary>
        /// Read elements from the buffer.
        /// </summary>
        /// <param name="destination">Destination buffer.</param>
        /// <param name="length">Number of elements to read.</param>
        public unsafe void Read (T* destination, int length) {
            // Check
            if (length > Length)
                throw new InvalidOperationException(@"Cannot read because buffer length is less than destination length");
            // Read
            var startIndex = read % Capacity;
            var endIndex = (startIndex + length) % Capacity;
            var firstCopyLength = endIndex < startIndex ? Capacity - startIndex : endIndex - startIndex;
            var secondCopyLength = length - firstCopyLength;
            fixed (T* baseAddress = buffer) {
                UnsafeUtility.MemCpy(destination, &baseAddress[startIndex], firstCopyLength * sizeof(T));
                UnsafeUtility.MemCpy(&destination[firstCopyLength], baseAddress, secondCopyLength * sizeof(T));
            }
            // Update marker
            read += length;
        }

        /// <summary>
        /// Write elements to the buffer.
        /// </summary>
        /// <param name="source">Source array.</param>
        public void Write (T[] source) => Write(source, 0, source.Length);

        /// <summary>
        /// Write elements to the buffer.
        /// </summary>
        /// <param name="source">Source array.</param>
        /// <param name="index">Source start index.</param>
        /// <param name="length">Number of elements to write.</param>
        public unsafe void Write (T[] source, int index, int length) {
            fixed (T* baseAddress = source)
                Write(&baseAddress[index], length);
        }

        /// <summary>
        /// Write elements to the buffer.
        /// </summary>
        /// <param name="source">Source array.</param>
        public unsafe void Write (NativeArray<T> source) => Write((T*)source.GetUnsafeReadOnlyPtr(), source.Length);

        /// <summary>
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="length">Number of elements to write.</param>
        public unsafe void Write (T* source, int length) {
            // Check
            if (length > Available)
                throw new InvalidOperationException(@"Cannot write because source length exceeds available space");
            // Write
            var startIndex = (write + 1) % Capacity;
            var endIndex = (startIndex + length) % Capacity;
            var firstCopyLength = endIndex < startIndex ? Capacity - startIndex : endIndex - startIndex;
            var secondCopyLength = length - firstCopyLength;
            fixed (T* baseAddress = buffer) {
                UnsafeUtility.MemCpy(&baseAddress[startIndex], source, firstCopyLength * sizeof(T));
                UnsafeUtility.MemCpy(baseAddress, &source[firstCopyLength], secondCopyLength * sizeof(T));
            }
            // Update marker
            write += length;
        }
        #endregion


        #region --Operations--
        private readonly T[] buffer;
        private int read;
        private int write;
        #endregion
    }
}