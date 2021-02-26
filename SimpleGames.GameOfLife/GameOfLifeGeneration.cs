using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

[assembly:CLSCompliant(true)]

namespace SimpleGames.GameOfLife {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Cell 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public struct GameOfLifeCell : IEquatable<GameOfLifeCell>, IComparable<GameOfLifeCell>, ISerializable {
    #region Create

    /// <summary>
    /// Standard constructor
    /// </summary>
    /// <param name="row">Row</param>
    /// <param name="column">Column</param>
    public GameOfLifeCell(int row, int column) {
      Row = row;
      Column = column;
    }

    /// <summary>
    /// Standard constructor
    /// </summary>
    public GameOfLifeCell((int row, int column) at)
      : this(at.row, at.column) { }

    /// <summary>
    /// Deserialization
    /// </summary>
    /// <param name="info">Info</param>
    /// <param name="context">Context</param>
    private GameOfLifeCell(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      Row = info.GetInt32("Row");
      Column = info.GetInt32("Column");
    }

    /// <summary>
    /// Deconstruction
    /// </summary>
    public void Deconstruct(out int row, out int column) {
      row = Row;
      column = Column;
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Compare
    /// </summary>
    public static int Compare(GameOfLifeCell left, GameOfLifeCell right) {
      int result = left.Row.CompareTo(right.Row);

      return result != 0
        ? result
        : left.Column.CompareTo(right.Column);
    }

    /// <summary>
    /// Column
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Row
    /// </summary>
    public int Row { get; }

    /// <summary>
    /// To String (debug)
    /// </summary>
    public override string ToString() => $"{Row} : {Column}";

    #endregion Public

    #region Operators

    /// <summary>
    /// From point 
    /// </summary>
    public static implicit operator GameOfLifeCell((int row, int column) point) => new GameOfLifeCell(point);

    /// <summary>
    /// Equal
    /// </summary>
    public static bool operator ==(GameOfLifeCell left, GameOfLifeCell right) => left.Equals(right);

    /// <summary>
    /// Not Equal
    /// </summary>
    public static bool operator !=(GameOfLifeCell left, GameOfLifeCell right) => !left.Equals(right);

    #endregion Operators

    #region IEquatable<Cell>

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(GameOfLifeCell other) => other.Row == Row && other.Column == Column;

    /// <summary>
    /// Equals
    /// </summary>
    public override bool Equals(object obj) => obj is GameOfLifeCell other && Equals(other);

    /// <summary>
    /// HashCode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() {
      unchecked {
        return (Row << 16) ^ Column;
      }
    }

    #endregion IEquatable<Cell>

    #region IComparable<Cell>

    /// <summary>
    /// Compare To Other
    /// </summary>
    public int CompareTo(GameOfLifeCell other) => Compare(this, other);

    #endregion IComparable<Cell>

    #region ISerializable

    /// <summary>
    /// Serialize
    /// </summary>
    public void GetObjectData(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      info.AddValue("Row", Row);
      info.AddValue("Column", Column);
    }

    #endregion ISerializable
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Game Of Life
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class GameOfLifeGeneration : IEquatable<GameOfLifeGeneration>, ICloneable, ISerializable {
    #region Private Data

    private HashSet<GameOfLifeCell> m_Live = new HashSet<GameOfLifeCell>();

    #endregion Private Data

    #region Algorithm

    private int NeighboursCount(GameOfLifeCell cell) {
      int result = 0;

      int row = cell.Row;
      int column = cell.Column;

      if (m_Live.Contains(new GameOfLifeCell(row + 1, column + 1)))
        result += 1;
      if (m_Live.Contains(new GameOfLifeCell(row + 1, column)))
        result += 1;
      if (m_Live.Contains(new GameOfLifeCell(row + 1, column - 1)))
        result += 1;

      if (m_Live.Contains(new GameOfLifeCell(row - 1, column + 1)))
        result += 1;
      if (m_Live.Contains(new GameOfLifeCell(row - 1, column)))
        result += 1;
      if (m_Live.Contains(new GameOfLifeCell(row - 1, column - 1)))
        result += 1;

      if (m_Live.Contains(new GameOfLifeCell(row, column + 1)))
        result += 1;
      if (m_Live.Contains(new GameOfLifeCell(row, column - 1)))
        result += 1;

      return result;
    }

    private void CoreNextGeneration() {
      unchecked {
        HashSet<GameOfLifeCell> agenda = new HashSet<GameOfLifeCell>();

        foreach (GameOfLifeCell cell in m_Live) {
          agenda.Add(new GameOfLifeCell(cell.Row + 1, cell.Column + 1));
          agenda.Add(new GameOfLifeCell(cell.Row + 1, cell.Column));
          agenda.Add(new GameOfLifeCell(cell.Row + 1, cell.Column - 1));

          agenda.Add(new GameOfLifeCell(cell.Row, cell.Column + 1));
          agenda.Add(new GameOfLifeCell(cell.Row, cell.Column));
          agenda.Add(new GameOfLifeCell(cell.Row, cell.Column - 1));

          agenda.Add(new GameOfLifeCell(cell.Row - 1, cell.Column + 1));
          agenda.Add(new GameOfLifeCell(cell.Row - 1, cell.Column));
          agenda.Add(new GameOfLifeCell(cell.Row - 1, cell.Column - 1));
        }

        HashSet<GameOfLifeCell> next = new HashSet<GameOfLifeCell>();

        foreach (GameOfLifeCell cell in agenda) {
          int count = NeighboursCount(cell);

          if (count == 3 || (count == 2 && m_Live.Contains(cell)))
            next.Add(cell);
        }

        m_Live = next;
      }
    }

    #endregion Algorithm

    #region Create

    // Deserialization
    private GameOfLifeGeneration(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      CultureInfo saved = CultureInfo.CurrentCulture;

      try {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Generation = info.GetInt32("Generation");

        var data = info
          .GetString("Field")
          .Split(';')
          .Select(line => line.Split(':'));

        foreach (var rec in data)
          m_Live.Add((int.Parse(rec[0]), int.Parse(rec[1])));
      }
      finally {
        CultureInfo.CurrentCulture = saved;
      }
    }

    /// <summary>
    /// Standard constructor
    /// </summary>
    public GameOfLifeGeneration() { }

    /// <summary>
    /// Create from Generation 
    /// </summary>
    /// <param name="cells">Initial generation</param>
    public GameOfLifeGeneration(IEnumerable<GameOfLifeCell> cells) {
      if (cells is null)
        throw new ArgumentNullException(nameof(cells));

      foreach (GameOfLifeCell cell in cells)
        m_Live.Add(cell);
    }

    /// <summary>
    /// Clone
    /// </summary>
    public GameOfLifeGeneration Clone() {
      GameOfLifeGeneration result = new GameOfLifeGeneration();

      foreach (GameOfLifeCell cell in m_Live)
        result.m_Live.Add(cell);

      return result;
    }

    /// <summary>
    /// From CSV
    /// </summary>
    public static GameOfLifeGeneration FromCsv(IEnumerable<string> lines) {
      if (lines is null)
        throw new ArgumentNullException(nameof(lines));

      GameOfLifeGeneration result = new GameOfLifeGeneration();

      Regex regex = new Regex(@"(?<row>-?[0-9]+)[0-9\-]+(?<col>-?[0-9]+)");

      foreach (string line in lines) {
        if (string.IsNullOrWhiteSpace(line))
          continue;

        int p = line.IndexOf('#');

        string rec = p > 0 ? line.Substring(0, p) : line;

        var match = regex.Match(rec);

        if (match.Success &&
            int.TryParse(match.Groups["row"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int row) &&
            int.TryParse(match.Groups["col"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int col))
          result.m_Live.Add((row, col));
        else
          throw new FormatException("Invalid csv.");
      }

      return result;
    }

    /// <summary>
    /// Try Parse field
    /// </summary>
    public static bool TryParse(string field, out GameOfLifeGeneration result) {
      result = null;

      if (field is null)
        return false;

      if (string.IsNullOrWhiteSpace(field)) {
        result = new GameOfLifeGeneration();

        return true;
      }

      result = new GameOfLifeGeneration();

      var lines = field
        .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(line => line.Trim());

      int row = -1;

      string empty = "_.";

      foreach (string line in lines) {
        row += 1;

        for (int col = 0; col < line.Length; ++col) {
          if (empty.Contains(line[col]))
            continue;

          result.m_Live.Add((row, col));
        }
      }

      return true;
    }

    /// <summary>
    /// Parse 
    /// </summary>
    public static GameOfLifeGeneration Parse(string field) => TryParse(field, out var result)
      ? result
      : throw new FormatException("");

    #endregion Create

    #region Public

    /// <summary>
    /// Number of Live cells
    /// </summary>
    public int Count => m_Live.Count;

    /// <summary>
    /// Generation
    /// </summary>
    public int Generation { get; private set; }

    /// <summary>
    /// Live Cells
    /// </summary>
    public IEnumerable<GameOfLifeCell> Cells => m_Live;

    /// <summary>
    /// Next generation
    /// </summary>
    public int Next(int generations) {
      if (generations < 0)
        throw new ArgumentOutOfRangeException(nameof(generations));

      for (int i = 0; i < generations; ++i)
        CoreNextGeneration();

      return Generation += generations;
    }

    /// <summary>
    /// Next generation
    /// </summary>
    public int Next() => Next(1);

    /// <summary>
    /// Cells
    /// </summary>
    /// <param name="cell">Cell to check</param>
    /// <returns>true if alive, false if dead</returns>
    public bool this[GameOfLifeCell cell] {
      get {
        return m_Live.Contains(cell);
      }
      set {
        if (value)
          m_Live.Add(cell);
        else
          m_Live.Remove(cell);
      }
    }

    /// <summary>
    /// Cells
    /// </summary>
    /// <returns>true if alive, false if dead</returns>
    public bool this[int row, int column] {
      get => this[new GameOfLifeCell(row, column)];
      set => this[new GameOfLifeCell(row, column)] = value;
    }

    /// <summary>
    /// Cells
    /// </summary>
    /// <returns>true if alive, false if dead</returns>
    public bool this[(int row, int column) at] {
      get => this[new GameOfLifeCell(at)];
      set => this[new GameOfLifeCell(at)] = value;
    }

    /// <summary>
    /// Lines Range
    /// </summary>
    public (int from, int to) LinesRange {
      get {
        if (m_Live.Count <= 0)
          return (0, 0);

        bool isFirst = true;

        int min = 0;
        int max = 0;

        foreach (GameOfLifeCell cell in m_Live) {
          if (isFirst) {
            min = cell.Row;
            max = cell.Row;

            isFirst = false;
          }
          else if (cell.Row > max)
            max = cell.Row;
          else if (cell.Row < min)
            min = cell.Row;
        }

        return (min, max + 1);
      }
    }

    /// <summary>
    /// Columns Range
    /// </summary>
    public (int from, int to) ColumnsRange {
      get {
        if (m_Live.Count <= 0)
          return (0, 0);

        bool isFirst = true;

        int min = 0;
        int max = 0;

        foreach (GameOfLifeCell cell in m_Live) {
          if (isFirst) {
            min = cell.Column;
            max = cell.Column;

            isFirst = false;
          }
          else if (cell.Column > max)
            max = cell.Column;
          else if (cell.Column < min)
            min = cell.Column;
        }

        return (min, max + 1);
      }
    }

    /// <summary>
    /// To Csv
    /// </summary>
    public IEnumerable<string> ToCsv() {
      var saved = CultureInfo.CurrentCulture;

      try {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        foreach (GameOfLifeCell cell in m_Live.OrderBy(item => item))
          yield return $"{cell.Row},{cell.Column}";
      }
      finally {
        CultureInfo.CurrentCulture = saved;
      }
    }

    /// <summary>
    /// To String (for given range)
    /// </summary>
    public string ToString((int from, int to) rows, (int from, int to) columns, char live = 'O', char dead = '.') {
      StringBuilder sb = new StringBuilder();

      for (int r = rows.from; r < rows.to; ++r) {
        if (sb.Length > 0)
          sb.AppendLine();

        for (int c = columns.from; c < columns.to; ++c)
          sb.Append(m_Live.Contains((r, c)) ? live : dead);
      }

      return sb.ToString();
    }

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() {
      var lines = LinesRange;
      var columns = ColumnsRange;

      if (lines.to - lines.from > 50)
        lines = (-25, 26);

      if (columns.to - columns.from > 100)
        columns = (-50, 51);

      return ToString(lines, columns);
    }

    #endregion Public

    #region IEquatable<GameOfLife>

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(GameOfLifeGeneration other) {
      if (other is null)
        return false;

      if (m_Live.Count != other.m_Live.Count)
        return false;

      return m_Live.SetEquals(other.m_Live);
    }

    /// <summary>
    /// Equals
    /// </summary>
    public override bool Equals(object o) => o is GameOfLifeGeneration other && Equals(other);

    /// <summary>
    /// Hash Code 
    /// </summary>
    public override int GetHashCode() => m_Live.Count;

    #endregion IEquatable<GameOfLife>

    #region ICloneable

    /// <summary>
    /// Clone
    /// </summary>
    object ICloneable.Clone() => this.Clone();

    #endregion ICloneable

    #region ISerializable

    /// <summary>
    /// Serialize
    /// </summary>
    public void GetObjectData(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      CultureInfo saved = CultureInfo.CurrentCulture;

      try {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        info.AddValue("Generation", Generation);
        info.AddValue("Field", string.Join(";", m_Live.Select(cell => $"{cell.Row}:{cell.Column}")));
      }
      finally {
        CultureInfo.CurrentCulture = saved;
      }
    }

    #endregion ISerializable
  }

}
