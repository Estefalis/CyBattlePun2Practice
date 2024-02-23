using System;

public class Kills : IComparable<Kills>
{
    public string m_playerName;
    public int m_kills;

    public Kills(string _playerName, int _kills)
    {
        m_playerName = _playerName;
        m_kills = _kills;
    }

    public int CompareTo(Kills other)
    {
        return other.m_kills - m_kills; //Sorts highest to lowest.
    }
}