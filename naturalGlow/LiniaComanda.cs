using Google.Cloud.Firestore;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class LiniaComanda
    {
        public LiniaComanda()
        {
            IdProducte = "-";
            NomProducte = "";
            Quantitat = 0;
            PreuUnitari = 0;
            Subtotal = 0;
        }

        public LiniaComanda(
            string idProducte,
            string nomProducte,
            int quantitat,
            double preuUnitari)
        {
            IdProducte = idProducte;
            NomProducte = nomProducte;
            Quantitat = quantitat;
            PreuUnitari = preuUnitari;
            Subtotal = quantitat * preuUnitari;
        }


        [FirestoreProperty]
        public string IdProducte { get; set; }

        [FirestoreProperty]
        public string NomProducte { get; set; }

        [FirestoreProperty]
        public int Quantitat { get; set; }

        [FirestoreProperty]
        public double PreuUnitari { get; set; }

        [FirestoreProperty]
        public double Subtotal { get; set; }


        public override string ToString()
        {
            return $"{NomProducte} x {Quantitat} = {Subtotal}€";
        }
    }
}