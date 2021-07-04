using System;
using System.Drawing;
using System.Windows.Forms;


namespace ChessDB
{
    public partial class Form1 : Form
    {
        #region Переменные
        /// <summary>
        /// Спрайты для шахматных фигур
        /// </summary>
        public Image chessSprites = new Bitmap(@"chess.png");
        /// <summary>
        /// Карта для игры
        /// </summary>
        public int[,] map;
        /// <summary>
        /// Переменная для хранения предыдущей нажатой кнопки
        /// </summary>
        Button prevButton;
        /// <summary>
        /// Ходил ли сейчас игрок?
        /// </summary>
        public bool IsMoving;
        /// <summary>
        /// Показывает, чей сейчас ход
        /// </summary>
        public int whoSTurn;
        /// <summary>
        /// Сам массив кнопок
        /// </summary>
        public Button[,] butts = new Button[8, 8];
        #endregion

        public Form1()
        {
            InitializeComponent();

            Init();
        }

        #region Инициализация
        /// <summary>
        /// Инициализация игры
        /// </summary>
        public void Init()
        {
            map = new int[8, 8]
                {
                {25,24,23,22,21,23,24,25 },
                {26,26,26,26,26,26,26,26},
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {16,16,16,16,16,16,16,16 },
                {15,14,13,12,11,13,14,15 }
                };
            CreateMap();
            IsMoving = false;
            whoSTurn = 1;
        }
        #endregion

        #region Создание карты
        /// <summary>
        /// Создание карты для игры
        /// </summary>
        public void CreateMap()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    butts[i, j] = new Button();

                    Button butt = new Button();
                    butt.Size = new Size(50, 50);
                    butt.Location = new Point(j * 50, i * 50);
                    switch ((i + j) % 2)
                    {
                        case 0: butt.BackColor = Color.Wheat; break;
                        case 1: butt.BackColor = Color.SaddleBrown; break;
                    }
                    #region Вырезание сегмента из картинки и вставка их в соответствующую клетку
                    switch (map[i, j] / 10)
                    {
                        case 1:
                            {
                                Image imgPart = new Bitmap(50, 50);
                                Graphics g = Graphics.FromImage(imgPart);
                                g.DrawImage(chessSprites, new Rectangle(0, 0, 50, 50), 0 + 150 * (map[i, j] % 10 - 1), 0, 150, 150, GraphicsUnit.Pixel);
                                butt.BackgroundImage = imgPart;
                                break;
                            }
                        case 2:
                            {
                                Image imgPart = new Bitmap(50, 50);
                                Graphics g = Graphics.FromImage(imgPart);
                                g.DrawImage(chessSprites, new Rectangle(0, 0, 50, 50), 0 + 150 * (map[i, j] % 10 - 1), 150, 150, 150, GraphicsUnit.Pixel);
                                butt.BackgroundImage = imgPart;
                                break;
                            }
                    }
                    #endregion
                    //Привязка к каждой кнопке обработчик на клик события OnFigureClick
                    butt.Click += new EventHandler(OnFigurePress);

                    this.Controls.Add(butt);

                    butts[i, j] = butt;
                }
            }
        }
        #endregion

        #region Нажатие на фигуру
        public void OnFigurePress(object sender, EventArgs e)
        {
            Button pressedButton = sender as Button;
            if (prevButton != null)
            {
                switch (((prevButton.Location.X + prevButton.Location.Y) / 50) % 2)
                {
                    case 0: prevButton.BackColor = Color.Wheat; break;
                    case 1: prevButton.BackColor = Color.SaddleBrown; break;
                }
            }
            //Проверка на то, чтобы выбирались только фигуры            
            if (map[pressedButton.Location.Y / 50, pressedButton.Location.X / 50] != 0 && map[pressedButton.Location.Y / 50, pressedButton.Location.X / 50] / 10 == whoSTurn)
            {
                CloseSteps();
                pressedButton.BackColor = Color.Coral;
                DeacivateAllbutts();
                pressedButton.Enabled = true;
                ShowSteps(pressedButton.Location.Y / 50, pressedButton.Location.X / 50, map[pressedButton.Location.Y / 50, pressedButton.Location.X / 50]);

                if (IsMoving)
                {
                    CloseSteps();
                    switch (((pressedButton.Location.X + pressedButton.Location.Y) / 50) % 2)
                    {
                        case 0: pressedButton.BackColor = Color.Wheat; break;
                        case 1:
                            pressedButton.BackColor = Color.SaddleBrown; break;
                    }
                    ActivateAllbutts();
                    IsMoving = false;
                }
                else
                    IsMoving = true;
            }
            else
            {
                if (IsMoving)
                {
                    int temp = map[pressedButton.Location.Y / 50, pressedButton.Location.X / 50];
                    map[pressedButton.Location.Y / 50, pressedButton.Location.X / 50] = map[prevButton.Location.Y / 50, prevButton.Location.X / 50];
                    map[prevButton.Location.Y / 50, prevButton.Location.X / 50] = temp;
                    pressedButton.BackgroundImage = prevButton.BackgroundImage;
                    prevButton.BackgroundImage = null;
                    IsMoving = false;
                    CloseSteps();
                    ActivateAllbutts();
                    SwitchPlayes();
                }
            }
            prevButton = pressedButton;
        }
        #endregion

        #region Показать возможные шаги
        /// <summary>
        /// Показывает шаги в завимисти от выбранной фигуры
        /// </summary>
        /// <param name="iCurrFigure"></param>
        /// <param name="JCurrFigure"></param>
        /// <param name="currFigure"></param>
        public void ShowSteps(int iCurrFigure, int JCurrFigure, int currFigure)
        {
            int dir = whoSTurn == 1 ? -1 : 1; // шаги пешки
            switch (currFigure % 10)
            {
                case 6://Если пешка
                    {
                        if (InsideBorder(iCurrFigure + 1 * dir, JCurrFigure))//Есть ли клетки, куда она может ходить
                        {
                            if (map[iCurrFigure + 1 * dir, JCurrFigure] == 0)
                            {
                                butts[iCurrFigure + 1 * dir, JCurrFigure].BackColor = Color.GreenYellow;
                                butts[iCurrFigure + 1 * dir, JCurrFigure].Enabled = true;
                                if ((whoSTurn == 1 && iCurrFigure == 6) || (whoSTurn == 2 && iCurrFigure == 1))
                                {
                                    if (InsideBorder(iCurrFigure + 1 * dir, JCurrFigure))
                                    {
                                        if (map[iCurrFigure + 2 * dir, JCurrFigure] == 0)
                                        {
                                            butts[iCurrFigure + 2 * dir, JCurrFigure].BackColor = Color.GreenYellow;
                                            butts[iCurrFigure + 2 * dir, JCurrFigure].Enabled = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (InsideBorder(iCurrFigure + 1 * dir, JCurrFigure + 1))//Тут проверка на то, можно ли забрать фигуру справа
                        {
                            if (map[iCurrFigure + 1 * dir, JCurrFigure + 1] != 0 && map[iCurrFigure + 1 * dir, JCurrFigure + 1] / 10 != whoSTurn)
                            {
                                butts[iCurrFigure + 1 * dir, JCurrFigure + 1].BackColor = Color.GreenYellow;
                                butts[iCurrFigure + 1 * dir, JCurrFigure + 1].Enabled = true;
                            }
                        }
                        if (InsideBorder(iCurrFigure + 1 * dir, JCurrFigure - 1))//Тут проверка на то, можно ли забрать фигуру слева
                        {
                            if (map[iCurrFigure + 1 * dir, JCurrFigure - 1] != 0 && map[iCurrFigure + 1 * dir, JCurrFigure - 1] / 10 != whoSTurn)
                            {
                                butts[iCurrFigure + 1 * dir, JCurrFigure - 1].BackColor = Color.GreenYellow;
                                butts[iCurrFigure + 1 * dir, JCurrFigure - 1].Enabled = true;
                            }
                        }
                        break;
                    }
                case 5:
                    {
                        ShowVerticalHorizontal(iCurrFigure, JCurrFigure);
                        break;
                    }
                case 4:
                    {
                        ShowHorseSteps(iCurrFigure, JCurrFigure);
                        break;
                    }
                case 3:
                    {
                        ShowDiagonal(iCurrFigure, JCurrFigure);
                        break;
                    }
                case 2:
                    {
                        ShowVerticalHorizontal(iCurrFigure, JCurrFigure);
                        ShowDiagonal(iCurrFigure, JCurrFigure);
                        break;
                    }
                case 1:
                    {
                        ShowVerticalHorizontal(iCurrFigure, JCurrFigure, true);
                        ShowDiagonal(iCurrFigure, JCurrFigure, true);
                        break;
                    }
            }
        }
        #endregion


        #region Смена хода
        /// <summary>
        /// Смена хода
        /// </summary>
        public void SwitchPlayes()
        {
            switch (whoSTurn)
            {
                case 1: whoSTurn = 2; break;
                case 2: whoSTurn = 1; break;
            }
        } 
        #endregion

        private void RestartBtn_Click(object sender, EventArgs e)
        {
            //this.Controls.Clear();
            foreach (var item in this.Controls)
            {
                if (item == RestartBtn) continue;
                this.Controls.Remove((Control)item);
            }

            Init();
        }

        #region Деактивация/активация всех кнопок
        private void DeacivateAllbutts()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    butts[i, j].Enabled = false;
                }
            }
        }
        private void ActivateAllbutts()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    butts[i, j].Enabled = true;
                }
            }
        }
        #endregion

        #region Внутри карты
        /// <summary>
        /// Проверяет входит ли клетка на карту
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="tj"></param>
        /// <returns></returns>
        public bool InsideBorder(int ti, int tj)
        {
            if (ti >= 8 || tj >= 8 || ti < 0 || tj < 0)
                return false;
            return true;
        } 
        #endregion

        /// <summary>
        /// Данный метод сбрасывает цвет клеток на стандартный
        /// </summary>
        private void CloseSteps()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    switch ((i + j) % 2)
                    {
                        case 0: butts[i, j].BackColor = Color.Wheat; break;
                        case 1: butts[i, j].BackColor = Color.SaddleBrown; break;
                    }
                }
            }
        }

        #region Ходы для "сильных" фигур
        public void ShowVerticalHorizontal(int IcurrFigure, int JcurrFigure, bool isOneStep = false)
        {
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (InsideBorder(i, JcurrFigure))
                {
                    if (!DeterminePath(i, JcurrFigure))//Если встретились с вражеской фигурой, то предотвращает показ
                        break;
                }
                if (isOneStep)//Проверка на короля
                    break;
            }
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (InsideBorder(i, JcurrFigure))
                {
                    if (!DeterminePath(i, JcurrFigure))
                        break;
                }
                if (isOneStep)
                    break;
            }
            for (int j = JcurrFigure + 1; j < 8; j++)
            {
                if (InsideBorder(IcurrFigure, j))
                {
                    if (!DeterminePath(IcurrFigure, j))
                        break;
                }
                if (isOneStep)
                    break;
            }
            for (int j = JcurrFigure - 1; j >= 0; j--)
            {
                if (InsideBorder(IcurrFigure, j))
                {
                    if (!DeterminePath(IcurrFigure, j))
                        break;
                }
                if (isOneStep)
                    break;
            }
        }

        public void ShowDiagonal(int IcurrFigure, int JcurrFigure, bool isOneStep = false)
        {
            int j = JcurrFigure + 1;
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (InsideBorder(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j < 7) j++;
                else break;
                if (isOneStep)//Проверка на короля
                    break;
            }
            j = JcurrFigure - 1;
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (InsideBorder(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j > 0) j--;
                else break;
                if (isOneStep)//Проверка на короля
                    break;
            }
            j = JcurrFigure - 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (InsideBorder(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j > 0) j--;
                else break;
                if (isOneStep)//Проверка на короля
                    break;
            }
            j = JcurrFigure + 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (InsideBorder(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j < 7) j++;
                else break;
                if (isOneStep)//Проверка на короля
                    break;
            }
        }

        public void ShowHorseSteps(int IcurrFigure, int JcurrFigure)
        {
            if (InsideBorder(IcurrFigure - 2, JcurrFigure + 1))
            {
                DeterminePath(IcurrFigure - 2, JcurrFigure + 1);
            }
            if (InsideBorder(IcurrFigure - 2, JcurrFigure - 1))
            {
                DeterminePath(IcurrFigure - 2, JcurrFigure - 1);
            }
            if (InsideBorder(IcurrFigure + 2, JcurrFigure + 1))
            {
                DeterminePath(IcurrFigure + 2, JcurrFigure + 1);
            }
            if (InsideBorder(IcurrFigure + 2, JcurrFigure - 1))
            {
                DeterminePath(IcurrFigure + 2, JcurrFigure - 1);
            }
            if (InsideBorder(IcurrFigure - 1, JcurrFigure + 2))
            {
                DeterminePath(IcurrFigure - 1, JcurrFigure + 2);
            }
            if (InsideBorder(IcurrFigure + 1, JcurrFigure + 2))
            {
                DeterminePath(IcurrFigure + 1, JcurrFigure + 2);
            }
            if (InsideBorder(IcurrFigure - 1, JcurrFigure - 2))
            {
                DeterminePath(IcurrFigure - 1, JcurrFigure - 2);
            }
            if (InsideBorder(IcurrFigure + 1, JcurrFigure - 2))
            {
                DeterminePath(IcurrFigure + 1, JcurrFigure - 2);
            }
        }
        #endregion

        #region Возможен ли ход
        /// <summary>
        /// Определеяет, является ли данная ячейка возможным ходом для фигуры
        /// </summary>
        /// <param name="IcurrFigure"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public bool DeterminePath(int IcurrFigure, int j)
        {
            if (map[IcurrFigure, j] == 0)//Ячейка пустая
            {
                butts[IcurrFigure, j].BackColor = Color.YellowGreen;
                butts[IcurrFigure, j].Enabled = true;
            }
            else//Ячейка не пустая
            {
                if (map[IcurrFigure, j] / 10 != whoSTurn)//Но в этой ячейке враг
                {
                    butts[IcurrFigure, j].BackColor = Color.YellowGreen;
                    butts[IcurrFigure, j].Enabled = true;
                }
                return false;
            }
            return true;
        } 
        #endregion
    }
}
