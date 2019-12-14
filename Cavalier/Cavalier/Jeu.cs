using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cavalier
{
    public partial class Jeu : Form
    {
        Simulation simulation;
        Simulation abandon;
        private MonBouton[,] plateau;
        private int[] pos = { 0, 0 };
        Image cavalierPic = Image.FromFile(@"images/cavalier.png");

        private List<int[]> possibilite = new List<int[]>();
        private List<int[]> posJoueur = new List<int[]>();
        private int annulerCounter = 5;
        private Color couleurPlateau;

        public Jeu()
        {
            InitializeComponent();
            
        }

        private void Jeu_Load(object sender, EventArgs e)
        {
            plateau = new MonBouton[8, 8];
            label3.Text = annulerCounter.ToString();
            this.Text = "Cavalier";

            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    MonBouton b = new MonBouton(l, c);
                    b.Location = new Point( 50 + l * 55, 50 + c * 55);
                    b.Size = new Size(55, 55);
                    b.Click += new EventHandler(this.buttonPlateau_Click);
                    this.Controls.Add(b);
                    plateau[l, c] = b;
                }
            }

            this.couleurPlateau = Color.LightSteelBlue;
            setCouleurs();

        }


        private int counterPos = 1;

        private void buttonPlateau_Click(object sender, EventArgs e)
        {
            
            MonBouton b = sender as MonBouton;


            if(b.atteignable == true && b.dejaPassé == false)
            {
                label5.Text = "";

                plateau[this.pos[0], this.pos[1]].Text = counterPos.ToString();
                this.counterPos++;

                plateau[this.pos[0], this.pos[1]].Image = null;

                this.pos[0] = b.L;
                this.pos[1] = b.C;
               
                b.dejaPassé = true;
                afficherJoueur();
                ajouterCurrentJoueurPos();
                afficherCasesUsées();
                afficherPossibilite(pos[0], pos[1]);
            }
            
        }


        private void setCouleurs()
        {
      
            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if ((c % 2 == 0 && l % 2 == 0) || (c % 2 != 0 && l % 2 != 0))
                    {
                        plateau[l, c].BackColor = this.couleurPlateau;
                    }
                        
                    else if ((c % 2 == 0 && l % 2 != 0) || (c % 2 != 0 && l % 2 == 0))
                    {
                        plateau[l, c].BackColor = Color.White;
                    }
                        
                }
            }
        }


        //Choix de la position par l'user
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
                ajouterCurrentJoueurPos();
                afficherPossibilite(pos[0], pos[1]); // le choix de position est bon  alors on affiche les possibilités

                label6.Text = "";
                button1.Enabled = false;
                button2.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;

            } else
            {
                label6.Text = "Please rentrer une position valide";
            }

        }

        // Cas de choix de positions aléatoires pour le cavalier
        private void button2_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            int posX = rnd.Next(0, 8);
            int posY = rnd.Next(0, 8);

            this.pos[0] = posX;
            this.pos[1] = posY;

            plateau[pos[0], pos[1]].dejaPassé = true;
            afficherJoueur();
            ajouterCurrentJoueurPos();

            button2.Enabled = false;
            button1.Enabled = false;

            // Affichage des possibilités de déplacement 
            afficherPossibilite(pos[0], pos[1]);

        }


        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        // Déplacements possibles en fonction de la position i, j
        private void deplacement(int i, int j)
        {
            int[] depi = new int[] { 2, 1, -1, -2, -2, -1, 1, 2 };
            int[] depj = new int[] { 1, 2, 2, 1, -1, -2, -2, -1 };


            for (int x = 0; x < depi.Length; x++)
            {
                int[] variablePos = { depi[x] + i, depj[x] + j };
                possibilite.Add(variablePos);
            }


        }


        // Affichage des différents placement possible a partir d'une position i, j
        private void afficherPossibilite(int i, int j)
        {
            deplacement(i, j);

            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    // on clear le plateau
                    
                    if (plateau[l, c].Text == "ICI")
                    {
                        plateau[l, c].Text = "";
                        plateau[l, c].atteignable = false;

                    }
                    
                  

                    foreach (int[] item in possibilite)
                    {
                        // Pour chaque bouton on vérifie si sa position est présente dans la liste de déplacement possibles
                        if(item[0] == plateau[l, c].L && item[1] == plateau[l, c].C )
                        {
                            bool successTest = Int32.TryParse(plateau[l, c].Text, out int inputY);

                            if (!successTest)
                            {
                                plateau[l, c].Text = "ICI";
                                plateau[l, c].atteignable = true;
                            }
                        }
                    }

                }
            }


            afficherResultat(); // On doit faire cette fonction la car on a besoin du tableau de possibilité
            // très important : cela vide notre liste de positions atteignables apres chaque coup
            possibilite.Clear();
        }


        private void afficherCasesUsées()
        {
            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (plateau[l, c].dejaPassé)
                    {
                        plateau[l, c].FlatStyle = FlatStyle.Flat;
                        plateau[l, c].FlatAppearance.BorderColor = Color.DarkRed;
                        plateau[l, c].FlatAppearance.BorderSize = 3;
                        //plateau[l, c].BackColor = Color.Tomato;
                    } else
                    {
                        plateau[l, c].FlatStyle = FlatStyle.Standard;
                        plateau[l, c].FlatAppearance.BorderColor = default(Color);
                        //plateau[l, c].BackColor = Color.Transparent;
                    }
                }
            }
        }

        private void afficherJoueur()
        {
        
            plateau[pos[0], pos[1]].Image = (Image)(new Bitmap(cavalierPic, new Size(50, 50)));

        }

        private void ajouterCurrentJoueurPos()
        {
            int[] currentPosJoueur = { this.pos[0], this.pos[1] };
            posJoueur.Add(currentPosJoueur);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            annulerCoup();
        }

        private void annulerCoup()
        {
            int length = posJoueur.Count - 1;

            if(annulerCounter > 0)
            {
                if (length >= 1) { // si la liste de positions est > à 1
                
                    plateau[posJoueur[length][0], posJoueur[length][1]].dejaPassé = false; // on efface la derniere position
                    posJoueur.RemoveAt(length); // On supprime la derniere position de la liste de position;

                    plateau[this.pos[0], this.pos[1]].Text = "";
                    this.counterPos--;
                    plateau[this.pos[0], this.pos[1]].Image = null;

                    this.pos[0] = posJoueur[posJoueur.Count - 1][0];
                    this.pos[1] = posJoueur[posJoueur.Count - 1][1];
            
                    afficherJoueur();
                    afficherPossibilite(this.pos[0], this.pos[1]);

                    afficherCasesUsées();
                    annulerCounter--;
                    label3.Text = annulerCounter.ToString();
                } else
                {
                    label5.Text = "Vous ne pouvez pas annuler la première position de votre cavalier";
                }
            }

            if (annulerCounter == 0)
            {
                button3.Enabled = false;
            } 

        }

        private Boolean gagné()
        {
            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if(!plateau[l, c].dejaPassé)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void afficherResultat()
        {
            if (gagné())
            {
                label4.Text = "BRAVO !!!";
            } else if (gameover())
            {
                label4.Text = "YOU LOST";
            }

        }

        private Boolean gameover()
        {
            // toute les possibilités (dont une des deux coords n'est pas égale à 0) proposées au joueur sont sur une case deja passée
            // annuler counter == 0

            bool indicateur = false;
           

            foreach(int[] pos in possibilite)
            {
                if (pos[0] <= 7 && pos[0] >= 0 && pos[1] >= 0 && pos[1] <= 7 && annulerCounter == 0) // si la possibilité est bien sur le tableau
                {
                    indicateur = plateau[pos[0], pos[1]].dejaPassé;
                    if (!indicateur)
                    {
                        break;
                    }
                }
            }

            return indicateur;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            reset();
        }

        private void reset()
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            annulerCounter = 5;
            this.counterPos = 1;
            label3.Text = annulerCounter.ToString();

            //plateau[pos[0], pos[1]].Text = "";
            
            posJoueur.Clear();
            possibilite.Clear();

            textBox1.Enabled = true;
            textBox2.Enabled = true;

            for (int l = 0; l < 8; l++)
            {
                for (int c = 0; c < 8; c++)
                {
                    plateau[l, c].dejaPassé = false;
                    plateau[l, c].atteignable = false;
                    plateau[l, c].Text = "";
                    plateau[l, c].Image = null;
                }
            }

            afficherCasesUsées();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.simulation = new Simulation(this, false);
            this.simulation.Show();
        }

        public int getPosX()
        {
            return this.pos[0];
        }

        public int getPosY()
        {
            return this.pos[1];
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.abandon = new Simulation(this, true);
            this.abandon.Show();
        }

        // ----- MENU ----- //


        private void règlesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Le but est de parcourir entierement le plateau sans repasser par la même case",
    "Rules");
        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created by Lin & Julien ",
    "Créateurs");
        }

        private void rEDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.couleurPlateau = Color.IndianRed;
            setCouleurs();
        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.couleurPlateau = Color.LightPink;
            setCouleurs();
        }

        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.couleurPlateau = Color.LightGreen;
            setCouleurs();
        }
    }
}
