using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot
{
    private int _botPosition;
    private String _botName;

    public Bot() { }
    public Bot(int position, string name) {
      _botPosition = position;
      _botName = name;
    }

    public int botPosition
    {
      get { return _botPosition; }
      set { _botPosition = value; }
    }


    public String botName
    {
      get { return _botName; }
      set { _botName = value; }
    }
}
