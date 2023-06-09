﻿namespace LancerComet.ImageSizeReader.Exceptions; 

public class UnexpectedDataPrecisionException : BaseException {
  public byte Precision { get; }

  public UnexpectedDataPrecisionException (byte precision) {
    this.Precision = precision;
  }
}