using System;
using System.IO;
using System.Text;
using LancerComet.ImageSizeReader.Exceptions;
using LancerComet.ImageSizeReader.Model;

namespace LancerComet.ImageSizeReader; 

public class ImageSizeReaderUtil {
  /// <summary>
  /// Get dimension of the image.
  /// </summary>
  /// <param name="fileStream"></param>
  /// <returns></returns>
  /// <exception cref="InvalidWidthOrHeightException"></exception>
  /// <exception cref="CoundNotDetermineDimensionsException"></exception>
  public static Size GetDimensions (Stream fileStream) {
    using var binaryReader = new BinaryReader(fileStream);
    try {
      var dimensions = GetDimensions(binaryReader);
      if (dimensions.Width == 0 || dimensions.Height == 0) {
        throw new InvalidWidthOrHeightException();
      }
      return dimensions;
    } catch (EndOfStreamException) {
      throw new CoundNotDetermineDimensionsException(4);
    }
  }

  private static Size GetDimensions (BinaryReader binaryReader) {
    var byte1 = binaryReader.ReadByte();
    var byte2 = binaryReader.ReadByte();

    if (byte1 == 0xff && byte2 == 0xd8) {
      return DecodeJfif(binaryReader);
    }

    if (byte1 == 0x89 && byte2 == 0x50) {
      var bytes = binaryReader.ReadBytes(6);
      if (bytes[0] == 0x4E && bytes[1] == 0x47 && bytes[2] == 0x0D && bytes[3] == 0x0A && bytes[4] == 0x1A && bytes[5] == 0x0A) {
        return DecodePng(binaryReader);
      }
    }

    if (byte1 == 0x47 && byte2 == 0x49) {
      var bytes = binaryReader.ReadBytes(4);
      if (bytes[0] == 0x46 && bytes[1] == 0x38 && (bytes[2] == 0x37 || bytes[2] == 0x39) && bytes[3] == 0x61) {
        return DecodeGif(binaryReader);
      }
    }

    if (byte1 == 0x52 && byte2 == 0x49) {
      var bytes = binaryReader.ReadBytes(10);
      var isWebP = (bytes[0] == 0x46 && bytes[1] == 0x46) &&
                   (bytes[6] == 0x57 && bytes[7] == 0x45 && bytes[8] == 0x42 && bytes[9] == 0x50);
      if (isWebP) {
        return DecodeWebp(binaryReader);
      }
    }

    if (byte1 == 0x42 && byte2 == 0x4d) {
      return DecodeBmp(binaryReader);
    }

    throw new UnsupportedFormatException();
  }

  private static Size DecodeBmp (BinaryReader binaryReader) {
    binaryReader.ReadInt32(); // Skip file size
    binaryReader.ReadInt32(); // Skip reserved
    binaryReader.ReadInt32(); // Skip offset to pixel data
    binaryReader.ReadInt32(); // Skip size of BITMAPINFOHEADER
    var width = binaryReader.ReadInt32(); // Width is next
    var height = binaryReader.ReadInt32(); // Height is after width
    return new Size(width, height);
  }

  private static Size DecodeGif (BinaryReader binaryReader) {
    int width = binaryReader.ReadInt16();
    int height = binaryReader.ReadInt16();
    return new Size(width, height);
  }

  private static Size DecodePng (BinaryReader binaryReader) {
    binaryReader.ReadBytes(8);
    var width = ReadLittleEndianInt32(binaryReader);
    var height = ReadLittleEndianInt32(binaryReader);
    return new Size(width, height);
  }

  private static Size DecodeJfif (BinaryReader binaryReader) {
    while (binaryReader.ReadByte() == 0xff) {
      var marker = binaryReader.ReadByte();
      var chunkLength = ReadLittleEndianInt16(binaryReader);
      if (chunkLength <= 2) {
        throw new MalformedImageException();
      }

      // 0xda = Start Of Scan
      if (marker == 0xda) {
        throw new CoundNotDetermineDimensionsException(1);
      }

      // 0xd9 = End Of Image
      if (marker == 0xd9) {
        throw new CoundNotDetermineDimensionsException(2);
      }

      // note: 0xc4 and 0xcc are missing. This is expected.
      if (marker is 0xc0 or 0xc1 or 0xc2 or 0xc3 or 0xc5 or 0xc6 or 0xc7 or 0xc8 or 0xc9 or 0xca or 0xcb or 0xcd or 0xce or 0xcf) {
        var precision = binaryReader.ReadByte();
        if (precision is 8 or 12 or 16) {
          var height = ReadLittleEndianInt16(binaryReader);
          var width = ReadLittleEndianInt16(binaryReader);
          return new Size(width, height);
        }

        throw new UnexpectedDataPrecisionException(precision);
      }

      // TODO: should perform many time to reduce amount of data being return at once
      binaryReader.ReadBytes(chunkLength - 2);
    }

    throw new CoundNotDetermineDimensionsException(3);
  }

  /// <summary>
  /// Based on code: https://github.com/image-size/image-size/blob/main/lib/types/webp.ts
  /// </summary>
  /// <param name="binaryReader"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  private static Size DecodeWebp (BinaryReader binaryReader) {
    var chunkHeader = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));  // 16
    binaryReader.ReadBytes(4);

    var buffer = binaryReader.ReadBytes(10);

    if (chunkHeader == "VP8X") {
      var extendedHeader = buffer[0];
      var validStart = (extendedHeader & 0xc0) == 0;
      var validEnd = (extendedHeader & 0x01) == 0;
      if (validStart && validEnd) {
        return WebPCalculateExtended(buffer, binaryReader);
      }
    }

    if (chunkHeader == "VP8 " && buffer[0] != 0x2f) {
      return WebPCalculateLossy(buffer);
    }

    var signature = BitConverter.ToString(buffer, 3, 3);
    if (chunkHeader == "VP8L" && signature != "9D-01-2A") {
      return WebPCalculateLossless(buffer);
    }

    throw new Exception("Invalid Webp");
  }

  private static Size WebPCalculateExtended (byte[] buffer, BinaryReader binaryReader) {
    using var ms = new MemoryStream(buffer);
    using var reader = new BinaryReader(ms);
    var width = 1 + (int)ReadUInt24(binaryReader); // You need to write a method to read 3 bytes as UInt24
    var height = 1 + (int)reader.ReadUInt32() >> 8;
    return new Size(width, height);
  }

  private static Size WebPCalculateLossless (byte[] buffer) {
    var width = 1 + ((buffer[2] & 0x3F) << 8) | buffer[1];
    var height = 1 + ((buffer[4] & 0xF) << 10) | (buffer[3] << 2) | ((buffer[2] & 0xC0) >> 6);
    return new Size(width, height);
  }

  private static Size WebPCalculateLossy (byte[] buffer) {
    using var ms = new MemoryStream(buffer);
    using var reader = new BinaryReader(ms);
    var width = reader.ReadInt16() & 0x3fff;
    var height = reader.ReadInt16() & 0x3fff;
    return new Size(width, height);
  }

  private static uint ReadUInt24 (BinaryReader reader) {
    var byte1 = reader.ReadByte();
    var byte2 = reader.ReadByte();
    var byte3 = reader.ReadByte();
    return (uint)((byte3 << 16) | (byte2 << 8) | byte1);
  }

  private static int ReadLittleEndianInt16 (BinaryReader binaryReader) {
    var bytes = new byte[4];
    bytes[1] = binaryReader.ReadByte();
    bytes[0] = binaryReader.ReadByte();
    return BitConverter.ToInt32(bytes, 0);
  }

  private static int ReadLittleEndianInt32 (BinaryReader binaryReader) {
    var bytes = new byte[4];
    bytes[3] = binaryReader.ReadByte();
    bytes[2] = binaryReader.ReadByte();
    bytes[1] = binaryReader.ReadByte();
    bytes[0] = binaryReader.ReadByte();

    return BitConverter.ToInt32(bytes, 0);
  }
}