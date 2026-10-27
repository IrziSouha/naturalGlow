using Google.Cloud.Firestore;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class LiniaFacturaProveidor
    {
        public LiniaFacturaProveidor()
        {
            IdProducte = "-";
            Quantitat = 0;
        }

        public LiniaFacturaProveidor(
            string idProducte,
            int quantitat)
        {
            IdProducte = idProducte;
            Quantitat = quantitat;
        }

        [FirestoreProperty]
        public string IdProducte { get; set; }

        [FirestoreProperty]
        public int Quantitat { get; set; }

        public override string ToString()
        {
            return $"{IdProducte} - {Quantitat} unitats";
        }
    }
}