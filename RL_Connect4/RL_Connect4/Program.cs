using System;
using System.Collections.Generic;
using System.IO;
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
		public char[,] dropPiece(int column, char player)
		{
			//Console.WriteLine("Dropping Piece");
			for (int i=0; i<6; i++)
			{
				char[,] temp = (char[,])board.Clone();
				//Console.WriteLine("Slot is: {0}", temp[i, column]);
				if(board[i,column] == '.')
				{
					temp[i, column] = player;
					return temp; 
				}
			}
			return board;
		}
		public void populateActions(char turn) //go through string and for each empty space, create a board where that space is filled and add it to actions
		{
			actions = new List<State>();
			for(int i=0; i<7; i++)
			{
				actions.Add(new State(dropPiece(i, turn), this));
			}
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
		public void addAction(State a)
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
		public bool isWin(char turn)
		{
			bool win = false;
			for(int i=0; i<7; i++)
			{
				for(int j=0; j<6; j++)
				{
					if(board[j,i] == turn)
					{
						win = true;
						for(int r=i; r<i+4; r++)//check right horizontal connect 4
						{
							if (r < 7)
							{
								if (board[j, r] != turn)
									win = false;
							}
							else
								win = false;
						}
						if (win)
							return true;
						win = true;
						for(int r=i; r>i-4; r--)
						{
							if (r >= 0)
							{
								if (board[j, r] != turn)
									win = false;
							}
							else
								win = false;
						}
						if (win)
							return true;
						win = true;
						for(int r=j; r<j+4; r++)
						{
							if (r < 6)
							{
								if (board[r, i] != turn)
									win = false;
							}
							else
								win = false;
						}
						if (win)
							return true;
						win = true;
						for(int r=j; r>j-4; r--)
						{
							if (r >= 0)
							{
								if (board[r, i] != turn)
									win = false;
							}
							else
								win = false;
						}
						if (win)
							return true;
						win = true;
						for(int r=i, t=j; r<i+4; r++, t++)
						{
							if (r < 7 && t < 6)
							{
								if (board[t, r] != turn)
									win = false;
							}
							else
								win = false;
						}
						if (win)
							return true;
						win = true;
						for (int r = i, t = j; r > i - 4; r--, t++)
						{
							if (r >= 0 && t < 6)
							{
								if (board[t, r] != turn)
									win = false;
							}
							else
								win = false;
						}
						if (win)
							return true;
					}
				}
			}
			return false;
		} //needs fixing
		public bool isFinished()
		{
			foreach(var obj in board)
			{
				if (obj == '.')
					return false;
			}
			return true;
		}
		public void print()
		{
			for(int i=5; i>=0; i--)
			{
				for(int j=0; j<7; j++)
				{
					if(j == 6)
					{
						Console.Write("{0}", board[i, j]);
						Console.WriteLine();
					}
					else 
						Console.Write("{0},", board[i, j]);
				}
			}
			Console.WriteLine("0,1,2,3,4,5,6");
		}
		public bool fullCol(int column)
		{
			return board[5, column] != '.';
		}
	}

	class Agent
	{
		//member
		private List<State> boards;
		private char player;
		private bool explore;
		private Random rng;
		private const double learnFactor = 0.1;

		//functions
		public Agent(char turn)
		{
			player = turn;
			boards = new List<State>();
			boards.Add(new State(makeEmpty(), null));
			rng = new Random();
			explore = true;
		}
		public Agent(char turn, List<State> array)
		{
			player = turn;
			boards = array;
			explore = false;
			rng = new Random();
		}
		public Agent(Agent temp)
		{
			boards = temp.getBoards();
			player = temp.getSide();
			explore = temp.getExplore();
			rng = new Random();
		}
		public char getSide()
		{
			return player;
		}
		public List<State> getBoards()
		{
			return boards;
		}
		public void setExplore(bool e)
		{
			explore = e;
		}
		public bool getExplore()
		{
			return explore;
		}

		public State makeMove(State prev)
		{
			//Console.WriteLine("{0}'s Turn.", player);
			State current = boards.Find(new Predicate<State>(n => Program.isEqual(prev.getBoard(), n.getBoard())));
			if (current == null) //if the list doesn't have that board, add it
			{
				boards.Add(prev);
				current = prev;
				//Console.WriteLine("List doesn't have board");
			}
			State parent = new State(makeEmpty(), 0.0);
			if(prev.getParent() != null)
				parent = boards.Find(new Predicate<State>(n => Program.isEqual(prev.getParent().getBoard(), n.getBoard())));
			if(parent == null)
			{
				boards.Add(prev.getParent());
			}

			//decide what the next move should be
			//Console.WriteLine("Found Board: {0}", current.getBoard());
			State next;
			if (current.getActions() != null) //if the board doesn't have the possible moves, make them then decide
			{
				current.getActions().Sort((x, y) => y.getScore().CompareTo(x.getScore()));
				/*Console.Write("Action values: ");
				foreach (var obj in current.getActions())
				{
					Console.Write("{0},", obj.getBoard());
				}
				Console.WriteLine();*/
				if (explore)
				{
					int random = rng.Next(0, current.getActions().Count);
					next = current.getActions()[random];
				}
				else
					next = current.getActions()[0];
			}
			else
			{
				//Console.WriteLine("No actions");
				current.populateActions(player);
				foreach (var obj in current.getActions())
				{
					boards.Add(obj);
				}
				//Console.WriteLine();
				int random = rng.Next(current.getActions().Count);
				next = current.getActions()[random];
			}
			//Console.WriteLine("{0}'s turn, Old board: {1}, New Board: {2}", player, prev.getBoard(), next.getBoard());
			//Console.WriteLine();

			return next;
		}
		public void reward(bool win, State final)
		{
			if (win)
			{
				final.setScore(1);
				State current = final.getParent();
				while (current != null)
				{ 
					if (current.getScore() < 1)
					{
						current.setScore(current.getScore() + current.getScore() * learnFactor);
						current = current.getParent();
					}
				}
			}
			else
			{
				final.setScore(final.getScore() - final.getScore() * learnFactor);
				State current = final.getParent();
				while (current != null)
				{
					current.setScore(current.getScore() - current.getScore() * learnFactor);
					current = current.getParent();
				}
			}
		}
		public char[,] makeEmpty()
		{
			char[,] temp = new char[6, 7];
			for(int i=0; i<6; i++)
			{
				for(int j=0; j<7; j++)
				{
					temp[i, j] = '.';
				}
			}
			return temp;
		}
	}

	class Program
	{
		public static bool isEqual(char[,] a, char[,] b)
		{
			//Console.WriteLine("Start isEqual");
			for(int i=0; i<6; i++)
			{
				for(int j=0; j<7; j++)
				{
					//Console.WriteLine("a[{0},{1}]: {2}", i, j, a[i, j]);
					//Console.WriteLine("b[{0},{1}]: {2}", i, j, b[i, j]);
					if (a[i, j] != b[i, j])
					{
						/*Console.WriteLine("End isEqual");
						Console.WriteLine("A is: ");
						print(a);
						Console.WriteLine("B is: ");
						print(b);*/
						return false;
					}
				}

				/*Console.WriteLine("A is: ");
				print(a);
				Console.WriteLine("B is: ");
				print(b);*/
			}
			//Console.WriteLine("End isEqual");
			return true;
		}
		static char[,] makeEmpty()
		{
			char[,] temp = new char[6, 7];
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					temp[i, j] = '.';
				}
			}
			return temp;
		}
		static void populateAgents(Agent x, Agent o, int gameCount)
		{
			Random rng = new Random();
			int count = 0;
			State start = new State(makeEmpty(), null);
			while (count < gameCount)
			{
				Console.WriteLine("Game Count: {0}", count);
				//set up game at each loop iteration
				State current = new State(start);
				var select = rng.Next(0, 2);
				bool xWin = false;
				bool oWin = false;
				char turn = 'R';
				//play the game
				while (!current.isFinished() && !oWin && !xWin)
				{
					if (turn == 'R')
					{
						current = x.makeMove(current);
						if (current.isWin(turn))
						{
							xWin = true;
						}
						turn = 'B';
					}
					else
					{
						current = o.makeMove(current);
						if (current.isWin(turn))
						{
							oWin = true;
						}
						turn = 'R';
					}
				}
				//current.print();
				//once game is over, have both agents give rewards
				if (xWin)
				{
					x.reward(true, current);
					o.reward(false, current);
					//Console.WriteLine("Red Wins\n");
					//Console.WriteLine();
				}
				if (oWin)
				{
					o.reward(true, current);
					x.reward(false, current);
					//Console.WriteLine("Blue Wins\n");
					//Console.WriteLine();
				}
				if (!xWin && !oWin)
				{
					//Console.WriteLine("Draw\n");
					//Console.WriteLine();
				}

				count++;
			}
		}
		static void saveData(Agent a)
		{
			//make a text file in the MyDocuments folder
			string filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			if (a.getSide() == 'R')
				filename += "\\agentR.csv";
			else
				filename += "\\agentB.csv";
			StreamWriter file = new StreamWriter(@filename, false);
			file.AutoFlush = true;

			//store board data in csv as board,score,parent
			int count = 0;
			foreach (var current in a.getBoards())
			{
				file.Write(current.getScore().ToString());
				file.Write(",");
				for(int i=0; i<6; i++)
				{
					for(int j=0; j<7; j++)
					{
						if (i == 5 && j == 6)
							file.Write(current.getBoard()[i, j]);
						else
						{
							file.Write(current.getBoard()[i, j]);
							file.Write("|");
						}
					}
				}
				file.Write(",");
				if (current.getParent() != null)
				{

					for (int i = 0; i < 6; i++)
					{
						for (int j = 0; j < 7; j++)
						{
							if (i == 5 && j == 6)
								file.Write(current.getParent().getBoard()[i, j]);
							else
							{
								file.Write(current.getParent().getBoard()[i, j]);
								file.Write("|");
							}
						}
					}
					file.Write(Environment.NewLine);
				}
				else
					file.Write("null" + Environment.NewLine);

				count++;
			}
			file.WriteLine("end");
		}//must fix
		static void print(char[,] board)
		{
			for (int i = 5; i >= 0; i--)
			{
				for (int j = 0; j < 7; j++)
				{
					if (j == 6)
					{
						Console.Write("{0}", board[i, j]);
						Console.WriteLine();
					}
					else
						Console.Write("{0},", board[i, j]);
				}
			}
		}
		public struct readIn
		{
			public char[,] board;
			public char[,] parent;
			public bool PisNull;
		};
		static Agent readData(char side)
		{
			//open file
			string filename;
			if (side == 'R')
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentR.csv";
			else
				filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentB.csv";
			StreamReader file = new StreamReader(@filename);

			//read in data, and create a new State List from it, along with the parent of each board
			var newItem = file.ReadLine();
			List<readIn> connect = new List<readIn>();
			List<State> boards = new List<State>();
			while (newItem != "end")
			{
				var thing = newItem.Split(',');
				var s = Convert.ToDouble(thing[0]);
				//Console.WriteLine("Score: {0}", s);
				var b = thing[1];
				//Console.WriteLine("Board: {0}", b);
				var p = thing[2];
				//Console.WriteLine("Parent: {0}", p);
				var Barray = b.Split('|');
				string[] Parray = new string[50];
				bool isnull = true;
				if (p != "null")
				{
					Parray = p.Split('|');
					isnull = false;
				}
				char[,] stuff = new char[6, 7];
				char[,] par = new char[6, 7];
				int count = 0;
				//Console.Write("Parent In: ");
				for (int i = 0; i < 6; i++) 
				{
					for (int j = 0; j < 7; j++)
					{
						//Console.WriteLine("Board readin: {0}", Barray[count]);
						//Console.WriteLine("Parent readin: {0}", Parray[count]);
						stuff[i, j] = Convert.ToChar(Barray[count]);
						if (!isnull)
						{
							par[i, j] = Convert.ToChar(Parray[count]);
							//Console.Write("{0},", par[i,j]);
						}
						count++;
					}
				}
				//Console.WriteLine();
				boards.Add(new State(stuff, s));
				readIn temp;
				temp.board = stuff;
				temp.parent = par;
				temp.PisNull = isnull;
				connect.Add(temp);
				newItem = file.ReadLine();
			}
			//Console.WriteLine("Done with read");

			/*foreach(var obj in boards)
			{
				Console.WriteLine("{0}, ", obj.getBoard());
			}*/
			//go through the connect list and connect all states to each other
			foreach (var obj in connect)
			{
				/*Console.WriteLine("Board: ");
				print(obj.board);
				Console.WriteLine("Parent: ");
				print(obj.parent);*/
				//Console.WriteLine("Looking for Parent: {0}", obj.parent);
				if (!obj.PisNull)
				{
					var x = boards.Find(new Predicate<State>(n => isEqual(n.getBoard(),obj.board)));//get the index of the current board
					if (x == null)
						Console.WriteLine("x is null");
					//Console.WriteLine("Board: {0}", x.getBoard());
					var y = boards.Find(new Predicate<State>(n => isEqual(n.getBoard(),obj.parent)));//get the index of the current board's parent
					//Console.WriteLine("Parent: {0}", y);
					if (y == null)
						Console.WriteLine("y is null");
					x.setParent(y);//set y as x's parent\
					//Console.WriteLine("Board: {0}, Parent:{1}", x.getBoard(), y.getBoard());
				}
			}
			file.Close();
			return new Agent(side, boards);
		}
		static void play(Agent agentB, Agent agentR)
		{
			//determine if the player is playing X or O, and get the opposing agent
			Agent comp;
			char agent;
			char human;
			Console.WriteLine("Red or Blue? ");
			var input = Console.ReadLine();
			if (input == "Red" | input == "red" | input == "R" | input == "r")
			{
				comp = agentB;
				agent = 'B';
				human = 'R';
			}
			else
			{
				comp = agentR;
				agent = 'R';
				human = 'B';
			}

			//while the player wants to play, continue playing games
			bool done = false;
			State start = new State(makeEmpty(), null);
			comp.setExplore(false);
			Console.WriteLine("Agent is: {0}", comp.getSide());
			Console.WriteLine("Human is: {0}", human);
			while (!done)
			{
				State current = new State(start);
				bool compWin = false;
				bool humWin = false;
				bool draw = false;
				char turn = 'R';

				Console.WriteLine("{0} goes first", turn);
				//play the game
				while (!current.isFinished() && !compWin && !humWin)
				{
					if (turn == agent)
					{
						current = comp.makeMove(current);
						if (current.isWin(agent))
							compWin = true;
						turn = human;
					}
					else if (turn == human)
					{
						current.print();
						Console.WriteLine("Input the column you want to drop it in. The left-most column is 0.");
						var index = Convert.ToInt32(Console.ReadLine());
						while (current.fullCol(index) | index < 0 | index > 6)
						{
							Console.WriteLine("Try again: ");
							index = Convert.ToInt32(Console.ReadLine());
						}
						current = new State(current.dropPiece(index,human), null);
						if (current.isWin(human))
							humWin = true;
						turn = agent;
					}
					if (current.isFinished() && !compWin && !humWin)
						draw = true;
					//Console.WriteLine("Is finished? {0}", current.isFinished());
				}

				done = true;
				//output the results of the game, if its not a draw, give rewards to agents
				if (compWin)
				{
					comp.reward(true, current);
					Console.WriteLine("\n");
					Console.WriteLine("You Lose.");
				}
				if (humWin)
				{
					comp.reward(false, current);
					Console.WriteLine("\n");
					Console.WriteLine("You Win!!!");
				}
				if (draw)
				{
					Console.WriteLine("Draw.");
				}
				Console.WriteLine("Go again? ");
				input = Console.ReadLine();
				if (input == "y" | input == "yes")
					done = false;
			}
		}

		static void Main(string[] args)
		{
			//create strings for where agent data is stored
			string filenameX = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentR.csv";
			string filenameO = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\agentB.csv";

			//check if the agent data exists, if so build those, otherwise build new agents
			Agent agentR;
			Agent agentB;
			if (File.Exists(@filenameX))
			{
				agentR = readData('R');
			}
			else
			{
				agentR = new Agent('R');
			}
			if (File.Exists(filenameO))
				agentB = readData('B');
			else
				agentB = new Agent('B');

			Console.WriteLine("Play? ");
			var input = Console.ReadLine();
			if (input == "y" | input == "yes")
			{
				agentB.setExplore(false);
				agentR.setExplore(false);
				play(agentB, agentR);
				saveData(agentB);
				saveData(agentR);
			}
			else
			{
				Console.WriteLine("How many games to play? ");
				var count = Convert.ToInt32(Console.ReadLine());
				populateAgents(agentR, agentB, count);
				Console.WriteLine("Save Data? ");
				input = Console.ReadLine();
				if (input == "y" | input == "yes")
				{
					saveData(agentB);
					saveData(agentR);
				}
			}

		}
	}
}
