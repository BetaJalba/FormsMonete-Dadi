using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GiocoMonete_Dadi
{
    public class CDado : ILanciabile 
    {
        protected static int step = 6;
        static int count = 1;
        
        protected string nome;

        public CDado() 
        {
            nome = "Dado" + count;
            count++;
        }

        public virtual int Lancia() 
        {
            Random random = new Random();
            return random.Next(step) + 1;
        }

        public string Nome() 
        {
            return nome;
        }
    }

    public class CDadoSpeciale : CDado 
    {
        bool canLancioSpeciale;

        public event EventHandler? OnSuperLancio;

        public CDadoSpeciale() : base() 
        {
            nome = "DadoSpeciale" + nome.Substring(4);
        }
        public override int Lancia()
        {
            Random random = new Random();
            int esito = random.Next(step) + 1;

            if (canLancioSpeciale && esito == 6) 
                OnSuperLancio?.Invoke(this, EventArgs.Empty);

            return esito;
        }


        public void TriggerLancioSpeciale() 
        {
            canLancioSpeciale = !canLancioSpeciale;
        }
    }

    public class CMonetaConMemoria : ILanciabile
    {
        static int step = 2; // 0 - croce; 1 - testa
        static int count = 1;

        int doppiaTestaCount;

        protected string nome;
        protected List<int> lanci;

        public EventHandler? OnDoppiaTesta;
        public EventHandler? OnTriplaCroce;

        public CMonetaConMemoria() 
        {
            doppiaTestaCount = 0;
            nome = "Moneta" + count;
            count++;
            lanci = new List<int>(3);
        }

        public virtual int Lancia() 
        {
            Random random = new Random();
            if (lanci.Count > 2)
                lanci.RemoveAt(0);
            lanci.Add(random.Next(step));
            ControllaEventi();
            return lanci.Last();
        }

        protected void ControllaEventi() 
        {
            if (lanci.Where(lancio => lancio == 0).ToArray().Length == 2) 
            {
                OnDoppiaTesta(this, EventArgs.Empty);
                doppiaTestaCount++; // TO DO aggiungere la tripla doppia testa in sacchetto
            }
                
            else if (lanci.Where(lancio => lancio == 1).ToArray().Length == 3)
                OnTriplaCroce(this, EventArgs.Empty);
        }

        public void ResettaMemoria() 
        {
            lanci = new List<int>(3);
        }

        public string Nome() 
        {
            return nome;
        }
    }

    public class CMonetaConPonderazione : CMonetaConMemoria 
    {
        int probabilità;

        public CMonetaConPonderazione() : base() 
        {
            Random random = new Random();
            int probabilità = random.Next(9) + 1; // possibilità a cui esce croce

            nome = "MonetaTruccata" + nome.Substring(6);
        }

        public override int Lancia()
        {
            Random random = new Random();

            if (random.Next(10) + 1 >= probabilità)
                lanci.Add(random.Next(1));
            else
                lanci.Add(random.Next(0));

            ControllaEventi();
            return lanci.Last();
        }
    }

    public class CSacchetto
    {
        bool canPlay;
        int oldMoneta;

        List<CDado> dadi;
        List<CMonetaConMemoria> monete;

        public event EventHandler<DadoEventArgs> OnMassimoDado;
        
        public event EventHandler? OnShouldRemove;

        public int Count { get; private set; }

        public CSacchetto() 
        {
            canPlay = true;
            oldMoneta = -1;

            dadi = new List<CDado>(5);
            monete = new List<CMonetaConMemoria>(10);

            

            Count = 15;

            for (int i = 0; i < 10; i++) 
            {
                Random random = new Random();

                if (i < 5) 
                {
                    if (random.Next(3) == 0) 
                    {
                        CDado dado = new CDadoSpeciale();
                        (dado as CDadoSpeciale).OnSuperLancio += SuperLancio;
                        dadi.Add(dado);
                    }
                    else
                        dadi.Add(new CDado());
                }

                if (random.Next(3) == 0)
                    monete.Add(new CMonetaConPonderazione()); 
                else 
                    monete.Add(new CMonetaConMemoria());


                monete.Last().OnDoppiaTesta += (sender, e) =>
                {
                    // gestione dell'old index
                    if (monete.IndexOf(sender as CMonetaConMemoria) < oldMoneta)
                        oldMoneta--;
                    else if (monete.IndexOf(sender as CMonetaConMemoria) == oldMoneta)
                        oldMoneta = -1;

                    monete.Remove(sender as CMonetaConMemoria);
                    Count--;

                    if (monete.Count == 0)
                        canPlay = false;
                };
                monete.Last().OnTriplaCroce += (sender, e) => canPlay = false;
            }
        }

        public string? LanciaRandom() 
        {
            Random random = new Random();
            string r;
            int last;

            if (monete.Count == 0)
                return null;

            if (random.Next(2) == 0)
            {
                last = random.Next(monete.Count);
                if (last != oldMoneta && oldMoneta != -1)
                    monete[oldMoneta].ResettaMemoria();
                oldMoneta = last;
                r = monete[last].Nome() + " ha ottenuto " + monete[last].Lancia();
            } else 
            {
                last = random.Next(dadi.Count);
                if (oldMoneta != -1)
                    monete[oldMoneta].ResettaMemoria();

                int esito = dadi[last].Lancia();

                if (dadi[last] is CDadoSpeciale && esito == 6)
                {
                    DadoEventArgs args = new DadoEventArgs();
                    args.id = last;
                    OnMassimoDado?.Invoke(null, args);
                    r = $"{dadi[last].Nome} ha eseguito un lancio a 6!";
                }
                else
                    r = dadi[last].Nome() + " ha ottenuto " + esito.ToString();
            }

            return r;
        }

        public string? LanciaRandom(int n)
        {
            string r;

            (dadi[n] as CDadoSpeciale).TriggerLancioSpeciale();
            r = dadi[n].Nome() + " ha ottenuto " + dadi[n].Lancia();
            
            if (dadi[n] is CDadoSpeciale)
                (dadi[n] as CDadoSpeciale).TriggerLancioSpeciale();
            
            return r;
        }

        private void SuperLancio(object? sender, EventArgs e) 
        {
            sender = new CDado();
        }

        public bool ContinuaGioco() 
        {
            return canPlay;
        }

        public string[] GetElements()
        {
            string[] r = new string[Count];
            int count = 0;

            dadi.ForEach(dado => { r[count] = dado.Nome(); count++; });
            monete.ForEach(moneta => { r[count] = moneta.Nome(); count++; });
            
            return r;
        }
    }

    public class DadoEventArgs : EventArgs 
    {
        public int id;
    }
}
