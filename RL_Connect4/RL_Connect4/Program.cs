using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RL_Connect4
{
	class State
	{
		//members
		private char[,] board; //string represents the current board state
		private double score; //the RL value of this action
		private List<State> actions; //array of possible action states
		private State parent;

		//functions
		public State(char[,] b, State p)
		{
			board = b;
			score = 0.5;
			actions = null;
			if (p != null)
				parent = p;
			else
				parent = null;
		}
		public State(char[,] b, double s)
		{
			board = b;
			score = s;
			actions = null;
			parent = null;
		}
		public State(State c)
		{
			actions = c.getActions();
			board = c.getBoard();
			score = c.getScore();
			parent = c.getParent();
		}
		public void dropPiece(int column, char player)
		{
			if (column > 7)
				return;

			for(int i=0; i<6; i++)
			{
				if(board[i,column] == '.')
				{
					board[i, column] = player;
					return;
				}
			}
		}
		public void populateActions(char turn) //go through string and for each empty space, create a board where that space is filled and add it to actions
		{
			if (actions == null)
				actions = new List<State>();
			
		}
		public double getScore()
		{
			return score;
		}
		public char[,] getBoard()
		{
			return board;
		}
		public State getParent()
		{
			return parent;
		}
		public List<State> getActions()
		{
			return actions;
		}
		public void setScore(double s)
		{
			score = s;
		}
		public void setBoard(char[,] b)
		{
			board = b;
		}
		public void setParent(State p)
		{
			parent = p;
			p.addAction(this);
		}
		/*public void addAction(State a)
		{
			int thiscount = 0;
			int count = 0;
			for (int i = 0; i < 9; i++)
			{
				if (board[i] != '.')
					thiscount++;
				if (a.getBoard()[i] != '.')
					count++;
			}
			if (count > thiscount)
			{
				if (actions == null)
				{
					actions = new List<State>();
					actions.Add(a);
				}
				else
					actions.Add(a);
				//Console.WriteLine("Adding '{0}' as an action on '{1}'", a.getBoard(), board);
			}
		}*/
		public bool isWin(char turn)
		{
			List<int> moves = new List<int>();
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					if (board[i,j] == turn)
					{
						moves.Add(i);
					}
				}
			}
			/*Console.Write("Moves of {0}: ", turn);
			foreach (var obj in moves)
			{
				Console.Write("{0},", obj);
			}
			Console.WriteLine();*/

			foreach (var obj in Win.winning)
			{
				bool win = true;
				for (int i = 0; i < 3; i++)
				{
					if (!moves.Contains(obj[i]))
						win = false;
				}
				if (win)
				{
					//Console.WriteLine("{0} wins", turn);
					return true;
				}
			}
			return false;
		}
		public bool isFinished()
		{
			return false;
		}
		public void print()
		{
			for (int i = 0; i < 9; i++)
			{
				if (board[i] != '.')
					Console.Write("{0}", board[i]);
				else
					Console.Write(" ");
				if ((i + 1) % 3 == 0)
				{
					Console.WriteLine();
					if (i != 8)
						Console.WriteLine("-----");
				}
				else
					Console.Write("|");
			}
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
		}
	}
}
