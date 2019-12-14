using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Cavalier
{
    public partial class Simulation : Form
    {
        Jeu jeu;
        private MonBouton[,] plateau;
        private int[] pos = { 1, 1 };
        //private String joueur = "X";
        private List<int[]> possibilite = new List<int[]>();
        private List<int> ordrePassageParCase = new List<int>();
        Image cavalierPic = Image.FromFile(@"images/cavalier.png");
        System.Windows.Forms.Timer timer;

        private int counterSimu = 1;
        private int secondes = 0;
        private int nbPas = 1;
        private Boolean triggerNonStop = false;
        private Boolean pause = false;
        private Boolean triggerRejouer = false;
        private Boolean isAbandonner = false;


        // Algo Euler
        static int[,] echec = new int[12, 12];
        static int[] depi = new int[] { 2, 1, -1, -2, -2, -1, 1, 2 };
        static int[] depj = new int[] { 1, 2, 2, 1, -1, -2, -2, -1 };


        public Simulation(Jeu j, Boolean isAbandonner)
        {
            InitializeComponent();
            this.jeu = j;
            this.isAbandonner = isAbandonner;
        }

        private void Simulation_Load(object sender, EventArgs e)
        {
            plateau = new MonBouton[8, 8];

            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    MonBouton b = new MonBouton(l, c);
                    b.Location = new Point(l * 55, c * 55);
                    b.Size = new Size(55, 55);
                    b.Click += new EventHandler(this.buttonPlateau_Click);
                    this.Controls.Add(b);
                    plateau[l, c] = b;
                }
            }

            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            label2.Visible = false;
            
            if (isAbandonner)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                listBox1.Enabled = false;
                label4.Visible = false;
                this.pos[0] = jeu.getPosX();
                this.pos[1] = jeu.getPosY();
                label2.Visible = true;
                this.secondes = 1000;
            }

        }

        private void buttonPlateau_Click(object sender, EventArgs e)
        {


        }

        //Choix de la position de début de la simulation par l'utilisateur

        private void button1_Click(object sender, EventArgs e)
        {
            bool successX = Int32.TryParse(textBox1.Text, out int inputX);
            bool successY = Int32.TryParse(textBox2.Text, out int inputY);

            if (successX && successY && inputX < 8 && inputX >= 0 && inputY < 8 && inputY >= 0)
            {
                this.pos[0] = inputX;
                this.pos[1] = inputY;


                plateau[pos[0], pos[1]].dejaPassé = true;
                afficherJoueur();

                label1.Text = "";
                button1.Enabled = false;
                button2.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
            }
            else
            {
                label1.Text = "Please rentrer une position valide";
            }
        }


        // Cas de choix de positions aléatoires pour le début de la simulation
        private void button2_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            int posX = rnd.Next(0, 8);
            int posY = rnd.Next(0, 8);

            this.pos[0] = posX;
            this.pos[1] = posY;

            plateau[pos[0], pos[1]].dejaPassé = true;
            afficherJoueur();

            button2.Enabled = false;
            button1.Enabled = false;
        }

        private void afficherJoueur()
        {
            plateau[pos[0], pos[1]].Image = (Image)(new Bitmap(cavalierPic, new Size(50, 50)));
            //plateau[pos[0], pos[1]].Text = joueur;
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {

            this.timer = new System.Windows.Forms.Timer();

            lancerSimulation();

            if ((setSecondes() && setNbPas()) || isAbandonner ) {
                if(!triggerNonStop)
                {
                    timer.Interval = secondes / this.nbPas;
                    timer.Tick += new EventHandler(timer_Tick);
                    timer.Start();

                    listBox1.Enabled = false;
                    textBox3.Enabled = false;
                    button4.Enabled = true;
                    button5.Enabled = true;
                    button6.Enabled = true;
                } else
                {
                    nonStopRun();
                }
                button3.Enabled = false;
            } else
            {
                label4.Text = "Veuillez choisir un temps et un nombre de pas";
            }


            if (counterSimu == 64)
            {
                timer.Stop();
            }
        }


        private void rejouer()
        {
            resetPlateau();
            counterSimu = 1;
        }


        private void nouvelleSimu()
        {

            timer.Stop();
            resetPlateau();
            listBox1.ClearSelected();
            ordrePassageParCase.Clear();

            counterSimu = 1;
            textBox3.Text = " ";
            secondes = 0;
            triggerNonStop = false;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = false;
            button5.Enabled = false;
            listBox1.Enabled = true;
            textBox3.Enabled = true;
            textBox1.Enabled = true;
            textBox2.Enabled = true;

        }


        private void resetPlateau()
        {
            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    plateau[l, c].dejaPassé = false;
                    plateau[l, c].Text = "";
                    plateau[l, c].Image = null;
                }
            }
            afficherCasesUsées();
        }



        private bool setNbPas()
        {
            if (!(listBox1.SelectedItem is null))
            {
                bool nbPasIsNumber = Int32.TryParse(listBox1.SelectedItem.ToString(), out int inputNbPas);

                if (nbPasIsNumber)
                {
                    this.nbPas = inputNbPas;
                }
                else
                {
                    triggerNonStop = true;
                }

            } else
            {
                return false;
            }

            return true;
        }



        private bool setSecondes()
        {
            bool success = Int32.TryParse(textBox3.Text, out int input);

            if (success)
            {
                this.secondes = input * 1000;
                return true;
            } else
            {
                label4.Text = "Rentrer un nombre valide";
                return false;
            }
        }

       
        private void timer_Tick(object sender, EventArgs e)
        {
            if(triggerRejouer)
            {
                rejouer();
                triggerRejouer = false;
            }

            if (!pause)
            {
                button4.Text = "Pause";
                for (; counterSimu < ordrePassageParCase.Count + 1;)
                {
                    chercherPos(counterSimu);
             
                    if (counterSimu % nbPas == 0)
                    {
                        afficherOrdre();
                        afficherJoueur();
                        afficherCasesUsées();
                    }
                    else if (counterSimu >= 60)
                    {
                        afficherOrdre();
                        afficherJoueur();
                        afficherCasesUsées();
                    }
                    counterSimu++;
                    break;
                }
            }
            else
            {
                button4.Text = "Play";
            }
        }



        private void nonStopRun()
        {
            for (int i = 1; i < ordrePassageParCase.Count + 1; i++)
            {
                chercherPos(i);
                afficherOrdre();
                afficherJoueur();
                afficherCasesUsées();
            }
        }


        private void afficherOrdre ()
        {

            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (plateau[l, c].ordre != 0 && plateau[l, c].dejaPassé)
                    {
                        plateau[l, c].Text = plateau[l, c].ordre.ToString();
                    } else
                    {
                        plateau[l, c].Text = "";
                    }
                }
            }

        }



        private void lancerSimulation()
        {
            int nb_fuite, min_fuite, lmin_fuite = 0;
            int i, j, k, l, ii, jj;

            ii = this.pos[0]+ 1;
            jj = this.pos[1] + 1;

            for (i = 0; i < 12; i++)
                for (j = 0; j < 12; j++)
                    echec[i, j] = ((i < 2 | i > 9 | j < 2 | j > 9) ? -1 : 0);

            i = ii + 1; j = jj + 1;
            echec[i, j] = 1;

            for (k = 2; k <= 64; k++)
            {
                for (l = 0, min_fuite = 11; l < 8; l++)
                {
                    ii = i + depi[l]; jj = j + depj[l];

                    nb_fuite = ((echec[ii, jj] != 0) ? 10 : fuite(ii, jj));

                    if (nb_fuite < min_fuite)
                    {
                        min_fuite = nb_fuite; lmin_fuite = l;
                    }
                }
                if (min_fuite == 9 & k != 64)
                {
                    //Console.WriteLine("***   IMPASSE   ***");
                    break;
                }
                i += depi[lmin_fuite]; j += depj[lmin_fuite];
                echec[i, j] = k;
            }

            for (i = 2; i < 10; i++)
            {
                for (j = 2; j < 10; j++)
                {
                    ordrePassageParCase.Add(echec[i, j]);
                }
                //Console.WriteLine();
            }
            distributionDesPositions();
            //Console.ReadKey();
        }


        static int fuite(int i, int j)
        {
            int n, l;

            for (l = 0, n = 8; l < 8; l++)
                if (echec[i + depi[l], j + depj[l]] != 0) n--;

            return (n == 0) ? 9 : n;
        }


        private void distributionDesPositions()
        {
            int counter = 0;

            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    plateau[l, c].ordre = ordrePassageParCase[counter];
                    counter++;   
                }
            }

        }


        private void chercherPos(int i)
        {
            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (plateau[l, c].ordre == i)
                    {
                        plateau[this.pos[0], this.pos[1]].Image = null;
                        this.pos[0] = l;
                        this.pos[1] = c;
                        plateau[l, c].dejaPassé = true;
                        break;
                    }
                }
            }
        }


        private void afficherCasesUsées()
        {
            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (plateau[l, c].dejaPassé)
                    {
                        plateau[l, c].BackColor = Color.Tomato;
                    }
                    else
                    {
                        plateau[l, c].BackColor = Color.Transparent;
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.pause = !pause;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.triggerRejouer = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            nouvelleSimu();
        }




    }
}
