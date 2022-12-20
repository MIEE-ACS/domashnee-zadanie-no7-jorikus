using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		//Поле на котором живет змея
		Entity field;
		// голова змеи
		Head head;
		// вся змея
		List<PositionedEntity> snake;
		// яблоко
		Apple apple;
		// бонус
		Bonus bonus;
		//количество очков
		int score;
		//таймер по которому 
		DispatcherTimer moveTimer;

		int d_time = 400;
		double speed;

		static public Random rand = new Random();
		
		//конструктор формы, выполняется при запуске программы
		public MainWindow()
		{
			InitializeComponent();
			
			snake = new List<PositionedEntity>();
			//создаем поле 300х300 пикселей
			field = new Entity(600, 600, "pack://application:,,,/Resources/snake.png");

			//создаем таймер 
			moveTimer = new DispatcherTimer();
			moveTimer.Interval = new TimeSpan(0, 0, 0, 0, d_time);
			moveTimer.Tick += new EventHandler(moveTimer_Tick);
		}

		//метод перерисовывающий экран
		private void UpdateField()
		{
			//обновляем положение элементов змеи
			foreach (var p in snake)
			{
				Canvas.SetTop(p.image, p.y);
				Canvas.SetLeft(p.image, p.x);
			}

			//обновляем положение яблока
			Canvas.SetTop(apple.image, apple.y);
			Canvas.SetLeft(apple.image, apple.x);

			//обновляем положение бонуса
			Canvas.SetTop(bonus.image, bonus.y);
			Canvas.SetLeft(bonus.image, bonus.x);
			
			//обновляем количество очков
			lblScore.Content = String.Format("{0}000", score);
		}

		//обработчик тика таймера. Все движение происходит здесь
		void moveTimer_Tick(object sender, EventArgs e)
		{
			//в обратном порядке двигаем все элементы змеи
			foreach (var p in Enumerable.Reverse(snake))
			{
				p.move();
			}

			//проверяем, что голова змеи не врезалась в тело
			foreach (var p in snake.Where(x => x != head))
			{
				//если координаты головы и какой либо из частей тела совпадают
				if (p.x == head.x && p.y == head.y)
				{
					//мы проиграли
					moveTimer.Stop();
					tbGameOver.Visibility = Visibility.Visible;
					return;
				}
			}

			//проверяем, что голова змеи не вышла за пределы поля
			if (head.x < 40 || head.x >= 540 || head.y < 40 || head.y >= 540)
			{
				//мы проиграли
				moveTimer.Stop();
				tbGameOver.Visibility = Visibility.Visible;
				return;
			}

			//проверяем, что голова змеи врезалась в яблоко
			if (head.x == apple.x && head.y == apple.y)
			{
				d_time -= 10;
				moveTimer.Interval = new TimeSpan(0, 0, 0, 0, d_time);
				//увеличиваем счет
				score++;
				//двигаем яблоко на новое место
				apple.move();
				// добавляем новый сегмент к змее
				var part = new BodyPart(snake.Last());
				canvas1.Children.Add(part.image);
				snake.Add(part);
			}

			if (rand.Next(50) == 1)
            {
				bonus.image.Visibility = Visibility.Visible;
            }
			if (head.x == bonus.x && head.y == bonus.y)
			{
				bonus.image.Visibility = Visibility.Hidden;
				d_time += d_time/100*25;
				moveTimer.Interval = new TimeSpan(0, 0, 0, 0, d_time);
				bonus.move();
			}
			speed = 1000f / d_time;
			speed_label.Content = "Speed: " + Convert.ToString(Math.Round(speed, 2));
			//перерисовываем экран
			UpdateField();
		}

		// Обработчик нажатия на кнопку клавиатуры
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
					head.direction = Head.Direction.UP;
					break;
				case Key.Down:
					head.direction = Head.Direction.DOWN;
					break;
				case Key.Left:
					head.direction = Head.Direction.LEFT;
					break;
				case Key.Right:
					head.direction = Head.Direction.RIGHT;
					break;
			}
		}

		// Обработчик нажатия кнопки "Start"
		private void button1_Click(object sender, RoutedEventArgs e)
		{
			d_time = 400;
			// обнуляем счет
			score = 0;
			// обнуляем змею
			snake.Clear();
			// очищаем канвас
			canvas1.Children.Clear();
			// скрываем надпись "Game Over"
			tbGameOver.Visibility = Visibility.Hidden;
			
			// добавляем поле на канвас
			canvas1.Children.Add(field.image);
			// создаем новое яблоко и добавлем его
			apple = new Apple(snake);
			canvas1.Children.Add(apple.image);
			// создаем бонус
			bonus = new Bonus(snake);
			canvas1.Children.Add(bonus.image);
			bonus.image.Visibility = Visibility.Hidden;
			// создаем голову
			head = new Head();
			snake.Add(head);
			canvas1.Children.Add(head.image);
			
			//запускаем таймер
			moveTimer.Start();
			UpdateField();

		}
		
		public class Entity
		{
			protected int m_width;
			protected int m_height;
			
			Image m_image;
			public Entity(int w, int h, string image)
			{
				m_width = w;
				m_height = h;
				m_image = new Image();
				m_image.Source = (new ImageSourceConverter()).ConvertFromString(image) as ImageSource;
				m_image.Width = w;
				m_image.Height = h;

			}

			public Image image
			{
				get
				{
					return m_image;
				}
			}
		}

		public class PositionedEntity : Entity
		{
			protected int m_x;
			protected int m_y;
			public PositionedEntity(int x, int y, int w, int h, string image)
				: base(w, h, image)
			{
				m_x = x;
				m_y = y;
			}

			public virtual void move() { }

			public int x
			{
				get
				{
					return m_x;
				}
				set
				{
					m_x = value;
				}
			}

			public int y
			{
				get
				{
					return m_y;
				}
				set
				{
					m_y = value;
				}
			}
		}

		public class Apple : PositionedEntity
		{
			List<PositionedEntity> m_snake;
			public Apple(List<PositionedEntity> s)
				: base(0, 0, 40, 40, "pack://application:,,,/Resources/fruit.png")
			{
				m_snake = s;
				move();
			}

			public override void move()
			{
				do
				{
					x = rand.Next(13) * 40 + 40;
					y = rand.Next(13) * 40 + 40;
					bool overlap = false;
					foreach (var p in m_snake)
					{
						if (p.x == x && p.y == y)
						{
							overlap = true;
							break;
						}
					}
					if (!overlap)
						break;
				} while (true);

			}
		}

		public class Bonus : PositionedEntity
		{
			List<PositionedEntity> m_snake;
			public Bonus(List<PositionedEntity> s)
				: base(0, 0, 40, 40, "pack://application:,,,/Resources/bonus.png")
			{
				m_snake = s;
				move();
			}

			public override void move()
			{
				do
				{
					x = rand.Next(13) * 40 + 40;
					y = rand.Next(13) * 40 + 40;
					bool overlap = false;
					foreach (var p in m_snake)
					{
						if (p.x == x && p.y == y)
						{
							overlap = true;
							break;
						}
					}
					if (!overlap)
						break;
				} while (true);

			}
		}

		public class Head : PositionedEntity
		{
			public enum Direction
			{
				RIGHT, DOWN, LEFT, UP, NONE
			};

			Direction m_direction;

			public Direction direction {
				set
				{
					m_direction = value;
					RotateTransform rotateTransform = new RotateTransform(90 * (int)value);
					image.RenderTransform = rotateTransform;
				}
			}

			public Head()
				: base(280, 280, 40, 40, "pack://application:,,,/Resources/head.png")
			{
				image.RenderTransformOrigin = new Point(0.5, 0.5);
				m_direction = Direction.NONE;
			}

			public override void move()
			{
				switch (m_direction)
				{
					case Direction.DOWN:
						y += 40;
						break;
					case Direction.UP:
						y -= 40;
						break;
					case Direction.LEFT:
						x -= 40;
						break;
					case Direction.RIGHT:
						x += 40;
						break;
				}
			}
		}

		public class BodyPart : PositionedEntity
		{
			PositionedEntity m_next;
			public BodyPart(PositionedEntity next)
				: base(next.x, next.y, 40, 40, "pack://application:,,,/Resources/body.png")
			{
				m_next = next;
			}

			public override void move()
			{
				x = m_next.x;
				y = m_next.y;
			}
		}
	}
}
