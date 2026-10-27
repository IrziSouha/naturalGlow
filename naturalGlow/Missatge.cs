using Google.Cloud.Firestore;
using System;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class Missatge
    {
        public Missatge()
        {
            Id = "-";
            IdEmissor = "";
            IdReceptor = "";
            Text = "";
            Data = DateTime.Now;
            Llegit = false;
        }

        public Missatge(
            string id,
            string idEmissor,
            string idReceptor,
            string text,
            DateTime data,
            bool llegit)
        {
            Id = id;
            IdEmissor = idEmissor;
            IdReceptor = idReceptor;
            Text = text;
            Data = data;
            Llegit = llegit;
        }

        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string IdEmissor { get; set; }

        [FirestoreProperty]
        public string IdReceptor { get; set; }

        [FirestoreProperty]
        public string Text { get; set; }

        [FirestoreProperty]
        public DateTime Data { get; set; }

        [FirestoreProperty]
        public bool Llegit { get; set; }

        public override string ToString()
        {
            return $"Missatge {Id} - Emissor: {IdEmissor} - Receptor: {IdReceptor} - Text: {Text}";
        }
    }
}