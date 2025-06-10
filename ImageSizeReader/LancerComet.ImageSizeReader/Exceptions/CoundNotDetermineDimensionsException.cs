namespace LancerComet.ImageSizeReader.Exceptions; 

public class CoundNotDetermineDimensionsException : BaseException {
  public int ErrorCode { get; }

  public CoundNotDetermineDimensionsException (int errorCode) {
    this.ErrorCode = errorCode;
  }
}