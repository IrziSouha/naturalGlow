using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using naturalGlow.MODEL;

namespace naturalGlow
{
    public interface IDAO
    {
        #region USERS
        Task AddUser(User user);
        Task UpdateUser(User user);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserById(string id);
        Task<List<User>> GetAllUsers();
        Task DeleteUser(User user);

        #endregion

        #region PRODUCTES
        Task AddProducte(Producte producte);
        Task UpdateProducte(Producte producte);
        Task<Producte> GetProducteById(string id);
        Task<List<Producte>> GetAllProductes();
        Task<List<Producte>> GetProductesByCategoria(string categoria);
        Task<List<Producte>> GetProductesWithStockBaix();
        Task DeleteProducte(Producte producte);
        #endregion

        #region COMANDA
        Task AddComanda(Comanda comanda);
        Task UpdateComanda(Comanda comanda);
        Task<Comanda> GetComandaById(string id);
        Task<List<Comanda>> GetAllComandes();
        Task<List<Comanda>> GetComandesByClient(string idClient);
        Task DeleteComanda(Comanda comanda);
        #endregion

        #region LINIA COMANDA
        Task AddLiniaComanda(LiniaComanda liniaComanda);
        Task UpdateLiniaComanda(LiniaComanda liniaComanda);
        Task<LiniaComanda> GetLiniaComandaById(string idProducte);
        Task<List<LiniaComanda>> GetAllLiniesComanda();
        Task DeleteLiniaComanda(LiniaComanda liniaComanda);
        #endregion

        #region FACTURA
        Task AddFactura(Factura factura);
        Task UpdateFactura(Factura factura);
        Task<Factura> GetFacturaById(string id);
        Task<List<Factura>> GetAllFactures();
        Task<Factura> GetFacturaByComanda(string idComanda);
        Task DeleteFactura(Factura factura);
        #endregion

        #region PROVEIDOR
        Task AddProveidor(Proveidor proveidor);
        Task UpdateProveidor(Proveidor proveidor);
        Task<Proveidor> GetProveidorById(string id);
        Task<List<Proveidor>> GetAllProveidors();
        Task DeleteProveidor(Proveidor proveidor);
        #endregion

        #region FACTURA PROVEIDOR

        Task AddFacturaProveidor(FacturaProveidor facturaProveidor);
        Task UpdateFacturaProveidor(FacturaProveidor facturaProveidor);
        Task<FacturaProveidor> GetFacturaProveidorById(string id);
        Task<List<FacturaProveidor>> GetAllFacturesProveidor();
        Task<List<FacturaProveidor>> GetFacturesProveidorByProveidor(string idProveidor);
        Task DeleteFacturaProveidor(FacturaProveidor facturaProveidor);

        #endregion

        #region LINIA FACTURA PROVEIDOR
        Task AddLiniaFacturaProveidor(LiniaFacturaProveidor liniaFacturaProveidor);
        Task UpdateLiniaFacturaProveidor(LiniaFacturaProveidor liniaFacturaProveidor);
        Task<LiniaFacturaProveidor> GetLiniaFacturaProveidorById(string idProducte);
        Task<List<LiniaFacturaProveidor>> GetAllLiniesFacturaProveidor();
        Task DeleteLiniaFacturaProveidor(LiniaFacturaProveidor liniaFacturaProveidor);

        #endregion

        #region DEVOLUCIONS
        Task AddDevolucio(Devolucio devolucio);
        Task UpdateDevolucio(Devolucio devolucio);
        Task<Devolucio> GetDevolucioById(string id);
        Task<List<Devolucio>> GetAllDevolucions();
        Task<List<Devolucio>> GetDevolucionsByComanda(string idComanda);
        Task<List<Devolucio>> GetDevolucionsByProducte(string idProducte);
        Task DeleteDevolucio(Devolucio devolucio);

        #endregion

        #region MISSATGES
        Task AddMissatge(Missatge missatge);
        Task UpdateMissatge(Missatge missatge);
        Task<Missatge> GetMissatgeById(string id);
        Task<List<Missatge>> GetAllMissatges();
        Task<List<Missatge>> GetMissatgesByEmissor(string idEmissor);
        Task<List<Missatge>> GetMissatgesByReceptor(string idReceptor);
        Task<List<Missatge>> GetConversa(
            string idUsuari1,
            string idUsuari2);
        Task DeleteMissatge(Missatge missatge);

        #endregion

    }

}
