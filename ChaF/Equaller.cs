using System;
using System.Collections.Generic;
using System.Text;


namespace Equaller
{
  public enum EquallerExceptionType
  {
    NONE,
    INTERNAL_ERROR,
    INVALID_DATA,
    INVALID_ENTRY_INFORMATION,
    ENTRY_INVALID_FORMAT,
    ENTRY_DUPLICATION,
    ENTRY_LABEL_INVALID_FORMAT,
    ENTRY_LABEL_UNKNOWN,
    ENTRY_LABEL_EMPTY,
    ENTRY_CONTENT_INVALID
  }

  public class EquallerData
  {
    /// <summary>
    ///   ラベル。
    /// </summary>
    public string Label {
      get; set;
    }
    /// <summary>
    ///   データ。
    /// </summary>
    public string Content {
      get; set;
    }


    public EquallerData(string label, string content)
    {
      Label = label;
      Content = content;
    }
  }

  public class EquallerEventArgs : EventArgs
  {
    /// <summary>
    ///   ラベル。
    /// </summary>
    public string Label {
      get; set;
    }
    /// <summary>
    ///   データ。
    /// </summary>
    public string Content {
      get; set;
    }
    /// <summary>
    ///   現在の行。
    /// </summary>
    public int Line {
      get; set;
    }
    /// <summary>
    ///   処理した行数。
    /// </summary>
    public int ProcessedLines {
      get; set;
    }


    public EquallerEventArgs()
    {
      Label = string.Empty;
      Content = string.Empty;
      ProcessedLines = 0;
    }

    public EquallerEventArgs(string label, string content, int line, int processedLines = 0)
    {
      Label = label;
      Content = content;
      Line = line;
      ProcessedLines = processedLines;
    }
  }


  public class EquallerException : Exception
  {
    /// <summary>
    ///   例外が発生した行。
    /// </summary>
    public int Line {
      get;
    }
    /// <summary>
    ///   例外の種別。
    /// </summary>
    public EquallerExceptionType Type {
      get;
    }


    public EquallerException(string message) : base(message)
    {
      Line = 0;
      Type = EquallerExceptionType.NONE;
    }

    public EquallerException(int line, EquallerExceptionType type)
    {
      Line = line;
      Type = type;
    }

    public EquallerException(int line, EquallerExceptionType type, string message) : base(message)
    {
      Line = line;
      Type = type;
    }
  }

  public class EquallerEntryInformation
  {
    /// <summary>
    ///   ラベル。
    /// </summary>
    public string Label {
      get; set;
    }
    /// <summary>
    ///   複数行のデータを許可するか。
    /// </summary>
    public bool AllowMultipleLines {
      get; set;
    }
    /// <summary>
    /// 
    /// </summary>
    public Action<EquallerEventArgs> Callback {
      get; set;
    }


    public EquallerEntryInformation()
    {
      Label = string.Empty;
      AllowMultipleLines = false;
      Callback = null;
    }

    public EquallerEntryInformation(string label, bool allowMultipleLines, Action<EquallerEventArgs> callback)
    {
      Label = label;
      AllowMultipleLines = allowMultipleLines;
      Callback = callback;
    }
  }

  public static class Equaller
  {
    public static EquallerData[] Parse(byte[] data, EquallerEntryInformation[] informationEntry, bool throughInvalidLabel = false)
    {
      // 引数が正しいか判定。
      if (data == null || data.Length <= 3) {
        throw new ArgumentException();
      }
      if (informationEntry == null || informationEntry.Length == 0) {
        throw new ArgumentException();
      }
      for (int i = 0; i < informationEntry.Length; ++i) {
        // ラベルが空、あるいは複数行エントリかつコールバック関数がnullの場合。
        if (informationEntry[i].Label == string.Empty || (informationEntry[i].AllowMultipleLines && informationEntry[i].Callback == null)) {
          throw new ArgumentException();
        }

        // ラベルが重複していないか判定。
        for (int j = i + 1; j < informationEntry.Length; ++j) {
          if (informationEntry[j].Label == informationEntry[i].Label) {
            throw new ArgumentException();
          }
        }
      }


      List<byte> dataFormatted = new List<byte>();
      List<EquallerData> result = new List<EquallerData>();
      int
        currentLine,
        lineStart,
        lineEnd,
        entryLabelStart = 0,
        entryLabelEnd,
        entryContentStart = 0,
        entryContentEnd = 0;
      bool
        isInvalid,
        isComment;
      string
        entry,
        content;
      EquallerEventArgs
        entryEventArgs;

      // List<byte>の指定範囲をbyte[]に変換する。
      byte[] ListToArray(List<byte> list, int index, int count)
      {
        byte[] temp = new byte[count];

        for (int i = 0; i < count; ++i, ++index) {
          temp[i] = list[index];
        }

        return temp;
      }


      // 不正な文字を含むか判定。CRを除いたデータをコピー。
      for (int i = 0; i < data.Length; ++i) {
        // 文字以外のデータでLF、CR、HT以外は許可しない。
        if ((data[i] > 0x7E || data[i] < 0x20) &&
            data[i] != 0x0A &&
            data[i] != 0x0D &&
            data[i] != 0x09) {
          throw new EquallerException(-1, EquallerExceptionType.INVALID_DATA);
        }

        // CRはコピーしない。
        if (data[i] != 0x0D) {
          dataFormatted.Add(data[i]);
        }
      }

      currentLine = 0;
      lineStart = 0;
      while (lineStart < dataFormatted.Count) {
        ++currentLine;

        // 行を読み込み。（LFまで読み込み。）
        isInvalid = true;
        isComment = false;
        lineEnd = lineStart;
        for (; lineEnd < dataFormatted.Count; ++lineEnd) {
          // LFの場合。
          if (dataFormatted[lineEnd] == 0x0A) {
            break;
          }
          // コメント行でも半角スペースでも水平タブでもない場合。
          else if (!isComment && dataFormatted[lineEnd] != 0x20 && dataFormatted[lineEnd] != 0x09) {
            if (isInvalid) {
              // 行頭が「#」の場合はコメント行。
              if (dataFormatted[lineEnd] == 0x23) {
                isComment = true;
                continue;
              }
              // 行頭が「=」の場合は不正書式判定。
              else if (dataFormatted[lineEnd] == 0x3D) {
                throw new EquallerException(currentLine, EquallerExceptionType.ENTRY_LABEL_EMPTY);
              }
              // 行頭が数字の場合は不正書式判定。
              else if (dataFormatted[lineEnd] >= 0x30 && dataFormatted[lineEnd] <= 0x39) {
                throw new EquallerException(currentLine, EquallerExceptionType.ENTRY_LABEL_INVALID_FORMAT);
              }

              isInvalid = false;
              entryLabelStart = lineEnd;
            }

            entryContentEnd = lineEnd;

          }
        }
        // 空行、スペースとタブのみの行、コメント行の場合はスルー。
        if (lineEnd == lineStart || isInvalid || isComment) {
          lineStart = lineEnd + 1;
          continue;
        }
        else {
          --lineEnd;
        }

        // ラベルを読み込み。（「=」まで読み込み。）
        isInvalid = true;
        entryLabelEnd = entryLabelStart;
        for (int i = entryLabelStart; i <= lineEnd; ++i) {
          if (dataFormatted[i] == 0x3D) {
            isInvalid = false;
            entryContentStart = i + 1;
            break;
          }
          else if (dataFormatted[i] != 0x20 && dataFormatted[i] != 0x09) {
            entryLabelEnd = i;
          }
        }
        // 不正書式の場合。（「YYY = XXX」のフォーマットになっていない。例：「XXX」）
        if (isInvalid) {
          throw new EquallerException(currentLine, EquallerExceptionType.ENTRY_INVALID_FORMAT);
        }
        // ラベルに不正な文字が含まれていないか判定。
        for (int i = entryLabelStart; i <= entryLabelEnd; ++i) {
          // 大文字・小文字・数字・アンダースコア以外を弾く。
          if ((dataFormatted[i] > 0x7A || dataFormatted[i] < 0x30) ||
              (dataFormatted[i] > 0x39 && dataFormatted[i] < 0x41) ||
              (dataFormatted[i] > 0x5A && dataFormatted[i] < 0x5F) ||
              dataFormatted[i] == 0x60) {
            throw new EquallerException(currentLine, EquallerExceptionType.ENTRY_LABEL_INVALID_FORMAT);
          }
        }

        // ラベルが有効か判定。
        isInvalid = true;
        entry = Encoding.UTF8.GetString(data, entryLabelStart, (entryLabelEnd - entryLabelStart + 1));
        foreach (EquallerEntryInformation item in informationEntry) {
          if (item.Label == entry) {
            isInvalid = false;

            // コンテンツが存在する場合。
            if (item.AllowMultipleLines) {
              content = Encoding.UTF8.GetString(ListToArray(dataFormatted, entryContentStart, dataFormatted.Count - entryContentStart));
            }
            else {
              if (entryContentStart < entryContentEnd) {
                // コンテンツの前にあるスペース・タブを除去。
                for (; entryContentStart <= entryContentEnd; ++entryContentStart) {
                  if (dataFormatted[entryContentStart] != 0x20 && dataFormatted[entryContentStart] != 0x09) {
                    break;
                  }
                }

                content = Encoding.UTF8.GetString(dataFormatted.ToArray(), entryContentStart, (entryContentEnd - entryContentStart) + 1);
              }
              else {
                content = string.Empty;
              }
            }

            entryEventArgs = new EquallerEventArgs(entry, content, currentLine);
            try {
              item?.Callback(entryEventArgs);
            }
            catch (EquallerException e) {
              throw e;
            }
            catch {
              throw new EquallerException(entryEventArgs.Line, EquallerExceptionType.ENTRY_CONTENT_INVALID);
            }

            // 処理した行をスルー。
            if (item.AllowMultipleLines && entryEventArgs.ProcessedLines != 0) {
              int processedLines = entryEventArgs.ProcessedLines;

              currentLine += processedLines - 1;
              for (; (lineEnd < dataFormatted.Count) && (processedLines != 0); ++lineEnd) {
                if (dataFormatted[lineEnd] == 0x0A) {
                  --processedLines;
                }
              }

              lineEnd -= 2;
            }

            // 重複データが存在するか確認。
            for (int i = 0; i < result.Count; ++i) {
              if (entry == result[i].Label) {
                throw new EquallerException(currentLine, EquallerExceptionType.ENTRY_DUPLICATION);
              }
            }
            result.Add(new EquallerData(entry, entryEventArgs.Content));
            break;
          }
        }
        // ラベルが存在しなかった場合。
        if (isInvalid && throughInvalidLabel == false) {
          throw new EquallerException(currentLine, EquallerExceptionType.ENTRY_LABEL_UNKNOWN);
        }

        lineStart = lineEnd + 2;
      }

      return result.ToArray();
    }
  }
}
