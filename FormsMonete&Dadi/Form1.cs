using GiocoMonete_Dadi;

namespace FormsMonete_Dadi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            avviato = false;
            nextScriptato = false;
            sacchetto = new CSacchetto();
            sacchetto.OnMassimoDado += (sender, e) => { nextScriptato = true; scriptatoN = e.id; };
            Array.ForEach(sacchetto.GetElements(), daAggiungere => listBox1.Items.Add(daAggiungere));
            currentItems = listBox1.Items.Count;
            timer1.Interval = 10;
        }

        int currentItems, scriptatoN;
        bool avviato, nextScriptato;
        CSacchetto sacchetto;

        private void button1_Click(object sender, EventArgs e)
        {
            avviato = !avviato;

            if (!(sender is Button)) 
            {
                timer1.Stop();
                MessageBox.Show("Hai finito!");
                this.Close();
                return;
            }
                

            if (avviato)
            {
                timer1.Start();
                (sender as Button).Text = "Stop";
            }
            else
            {
                timer1.Stop();
                (sender as Button).Text = "Avvia";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!nextScriptato) 
            {
                label1.Text = sacchetto.LanciaRandom();
                nextScriptato = false;
            }
            
            fineLancio();
        }

        private void fineLancio() 
        {
            if (sacchetto.Count != currentItems) 
            {
                listBox1.Items.Clear();
                Array.ForEach(sacchetto.GetElements(), daAggiungere => listBox1.Items.Add(daAggiungere));
            }
                
            // controlla condizioni di uscita
            if (!sacchetto.ContinuaGioco())
                button1_Click(this, EventArgs.Empty);
        }
    }
}