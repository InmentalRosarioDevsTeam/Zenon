using System;
using System.Collections.Generic;

public class PriceManager
{
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public class Premio
    {
        public int index;
        public int id;
        public string descripcion;
        public int cantidad;
        public string lista;


        //Sembrado
        public bool entregado;
        public DateTime fecha;
    }

    public class ListPremios
    {
        public Queue<Premio> premios;
        public int premiosTotalCount;
        public DateTime initialDate;
        public DateTime finalDate;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //PUBLIC_MEMBERS
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------

 
    //public bool forzarEntregaPremios = false;

    //----------------------------------------------------------------------------------------------------------------------------------------
    //PUBLIC_PROPIETIES
    //----------------------------------------------------------------------------------------------------------------------------------------

    static public PriceManager instance
    {
        get
        {
            if (_instance == null)
                _instance = new PriceManager();

            return _instance;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------
    //CONSTRUCTOR
    //----------------------------------------------------------------------------------------------------------------------------------------

    public PriceManager()
    {
        calculate();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------
    //PUBLIC_METHODS
    //----------------------------------------------------------------------------------------------------------------------------------------

    public void Empty()
    {

    }

    public void calculate()
    {
        int i;
        Premio p;

        //Premios

        i = 1;
        p = null;
        _premios = new Dictionary<string, ListPremios>();

        while (true)
        {
            if (Config.Instance.HasElement("premio" + i, "premios"))
            {
                p = new Premio()
                {
                    index = i,
                    id = Config.Instance.GetInt("premio" + i, 0, "premios", "id"),
                    cantidad = Config.Instance.GetInt("premio" + i, 0, "premios", "cantidad"),
                    descripcion = Config.Instance.GetString("premio" + i, "", "premios", "descripcion"),
                    lista = Config.Instance.GetString("premio" + i, "", "premios", "lista")
                };

                ListPremios ps;
                if (!_premios.TryGetValue(Config.Instance.GetString("premio" + i, "", "premios", "lista"), out ps))
                {
                  
                    ps = new ListPremios() { premios = new Queue<Premio>() };
                    _premios[Config.Instance.GetString("premio" + i, "", "premios", "lista")] = ps;
                }

                for (int j = 0; j < p.cantidad; j++)
                {
                    ps.premios.Enqueue(p);
                }
                DateTime id;
                DateTime fd;
                checkIfListaIsInNowDateRange(Config.Instance.GetString("premio" + i, "", "premios", "lista"), out id, out fd);
                ps.initialDate = id;
                ps.finalDate = fd;

                i++;
            }
            else
                break;
        }

        //Premios Sembrados.

        i = 1;
        p = null;
        _premiosSembrados = new List<Premio>();

        while (true)
        {
            if (Config.Instance.HasElement("premio" + i, "premiosSembrados"))
            {
                p = new Premio()
                {
                    index = i,
                    id = Config.Instance.GetInt("premio" + i, 0, "premiosSembrados", "id"),
                    cantidad = Config.Instance.GetInt("premio" + i, 0, "premiosSembrados", "cantidad"),
                    descripcion = Config.Instance.GetString("premio" + i, "", "premiosSembrados", "descripcion"),
                    entregado = Config.Instance.GetBoolean("premio" + i, false, "premiosSembrados", "entregado"),
                    fecha = Convert.ToDateTime(Config.Instance.GetString("premio" + i, DateTime.Now.ToString(), "premiosSembrados", "fecha"))
                };

                _premiosSembrados.Add(p);

                i++;
            }
            else
                break;
        }

        //Mesclo el array.
        foreach (KeyValuePair<string, ListPremios> item in _premios)
        {
            randomize(item.Value.premios);
            item.Value.premiosTotalCount = item.Value.premios.Count;
        }
    }

    public bool checkIfListaIsInNowDateRange( string lista , out DateTime initialDate , out DateTime finalDate )
    {
        //Asigno la fecha inicial al dia de hoy y luego mofico la hora.
        DateTime n = DateTime.Now;

        initialDate = DateTime.Now;
        finalDate = DateTime.Now;
        initialDate = new DateTime(n.Year, n.Month, n.Day, Config.Instance.GetInt("hora_inicio_" + lista, 8), 0, 0);

        DateTime day = new DateTime(n.Year, n.Month, n.Day);

        //Si la hora inicial es mas alta que la hora final supongo que lo que se quizo hacer es una premiacion de un dia para el otro.
        //Entonces sumo a la hora final un dia.
        if (Config.Instance.GetInt("hora_inicio_" + lista, 8) > 
            Config.Instance.GetInt("hora_final_" + lista, 18))
            day = day.AddDays(1);

        if (Config.Instance.GetInt("hora_final_" + lista, 18) == 0)
            finalDate = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);
        else
            finalDate = new DateTime(day.Year, day.Month, day.Day, Config.Instance.GetInt("hora_final_" + lista, 18), 0, 0);

        //Si la fecha inicial es menor a la actual cambio la hora a la actual.
        DateTime horaActual = DateTime.Now;
        if (horaActual.Ticks > initialDate.Ticks)
            initialDate = horaActual;

        //Hora actual.
        DateTime date = DateTime.Now;

        //Si la hora actual no esta dentro de la franja horaria devuelvo que no hay premio.
        if (date < initialDate ||
            date > finalDate)
        {
            return false;
        }

        return true;
    }

    public Premio getPremio()
    {
        /*
        La logica es la siguiente.
        Se saca una hora inicial y una hora final. De ahi se saca cada cuantos segundos se tiene que entregar un premio dependiendo la cantidad de premios.
        De ahi si la hora actual es mayor a ese intervalo de tiempo * por la cantidad de premios se entrega.

        El unico comportamiento RARO es que se deje sin usar mucho tiempo la maquina y entregue varios premios juntos.
        Una forma de solucionarlo es haciendo la matrix de "premios por hora" de entrada y sacar todos los "viejos" una vez entregado un premio.
        Pero se perderian varios premios.

        */

        DateTime initialDate = default(DateTime);
        DateTime finalDate = default(DateTime);
        List<ListPremios> listasPremiosActuales = new List<ListPremios>();
        //Hora actual.
        DateTime date = DateTime.Now;

        foreach (KeyValuePair <string, ListPremios > item in _premios)
        {
            DateTime i_date = item.Value.initialDate;
            DateTime f_date = item.Value.finalDate;

            if (date > i_date &&
                date < f_date)
            {
                if (initialDate == default(DateTime) || i_date < initialDate)
                    initialDate = i_date;

                if (finalDate == default(DateTime) || f_date > finalDate)
                    finalDate = f_date;

                listasPremiosActuales.Add(item.Value);
            }
        }

        if (listasPremiosActuales.Count == 0)
            return getPremioDefecto();

        UnityEngine.Debug.Log("listaPremios.premiosTotalCount -- > " + listasPremiosActuales.Count);
        ListPremios listaPremios = new ListPremios() { premios = new Queue<Premio>() };

        for (int i = 0; i < listasPremiosActuales.Count; i++)
        {
            listaPremios.premiosTotalCount += listasPremiosActuales[i].premiosTotalCount;

            foreach (Premio p in listasPremiosActuales[i].premios)
                listaPremios.premios.Enqueue(p);
        }

        randomize(listaPremios.premios);

        if (listaPremios.premios.Count == 0)
            return getPremioDefecto();

        //Saco el tiempo total de la activacion.
        double tiempo = finalDate.Subtract(initialDate).TotalSeconds;

        //Saco el tiempo que tengo que ir dando premios.
        double tiempoEntregaPremio = tiempo / listaPremios.premiosTotalCount;
        double tiempoDelProximoPremio = (listaPremios.premiosTotalCount - listaPremios.premios.Count) * tiempoEntregaPremio;

        /*
        UnityEngine.Debug.Log("tiempoEntregaPremio:" + new DateTime((long)tiempoEntregaPremio).ToString("dd/MM/yyyy HH:mm:ss"));
        UnityEngine.Debug.Log("lapso:" + finalDate.Subtract(initialDate).TotalSeconds);
        UnityEngine.Debug.Log("initialDate:" + initialDate.ToString("dd/MM/yyyy HH:mm:ss"));
        UnityEngine.Debug.Log("finalDate:" + finalDate.ToString("dd/MM/yyyy HH:mm:ss"));
        UnityEngine.Debug.Log("tiempoDelProximoPremio:" + new DateTime((long)tiempoDelProximoPremio).ToString("dd/MM/yyyy HH:mm:ss"));
        UnityEngine.Debug.Log("listaPremios.premios.Count:" + listaPremios.premios.Count);
        UnityEngine.Debug.Log("listaPremios.premiosTotalCount:" + listaPremios.premiosTotalCount);
        UnityEngine.Debug.Log("(date.Ticks:" + new DateTime(date.Ticks).ToString("dd/MM/yyyy HH:mm:ss"));
        UnityEngine.Debug.Log("tiempoDelProximoPremio:" + new DateTime((long)tiempoDelProximoPremio).ToString("dd/MM/yyyy HH:mm:ss"));
        UnityEngine.Debug.Log(date.Ticks >= tiempoDelProximoPremio);
        UnityEngine.Debug.Log("listaPremios.premiosTotalCount -- > " + listaPremios.premiosTotalCount);
        UnityEngine.Debug.Log("listaPremios.premios.Count -- > " + listaPremios.premios.Count);
        UnityEngine.Debug.Log("tiempoDelProximoPremio -- > " + tiempoDelProximoPremio);
        UnityEngine.Debug.Log(date.ToString("HH:mm:ss") + " ---> " + initialDate.AddSeconds(tiempoDelProximoPremio).ToString("HH:mm:ss"));
        UnityEngine.Debug.Log(date >= initialDate.AddSeconds(tiempoDelProximoPremio));
        */

        //Si el tiempo es mayor que el tiempo que tiene para entregar los premios
        if (date >= initialDate.AddSeconds(tiempoDelProximoPremio) )
        {
            if (listaPremios != null)
            {
                Premio p = listaPremios.premios.Dequeue();

                List<Premio> temp = new List<Premio>(_premios[p.lista].premios);
                temp.Remove(p);
                _premios[p.lista].premios = new Queue<Premio>(temp);

                //Guardo el premio.
                p.cantidad--;
                Config.Instance.SetInt("premio" + p.id, p.cantidad, "premios", "cantidad");

                return p;
            }

            return getPremioDefecto();
        }

        return getPremioDefecto();
    }

    public int getIdUnico()
    {
        int id = Config.Instance.GetInt("id_unico", 1);

        id++;
        Config.Instance.SetInt("id_unico", id);

        return id;
    }

    private Premio getPremioDefecto()
    {
        return new Premio() { id = 0, descripcion = getDescripcionPremioDefecto() };
    }

    private string getDescripcionPremioDefecto()
    {
        return Config.Instance.GetString("descripcionPremioPorDefecto", "No hay mas premios");
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //PRIVATE_MEMBERS
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------
    //PRIVATE_PROPIETIES
    //----------------------------------------------------------------------------------------------------------------------------------------

    private Dictionary<string, ListPremios> _premios;
    private List<Premio> _premiosSembrados;
    static private PriceManager _instance;

    //----------------------------------------------------------------------------------------------------------------------------------------
    //PRIVATE_METHODS
    //----------------------------------------------------------------------------------------------------------------------------------------

    static private Queue<Premio> randomize(Queue<Premio> queue)
    {
        Premio[] array = queue.ToArray();
        Premio temp;
        int tempOffset;

        for (int i = array.Length - 1; i >= 0; i--)
        {
            tempOffset = (int)(UnityEngine.Random.Range(0f, 1f) * i);
            temp = array[i];
            array[i] = array[tempOffset];
            array[tempOffset] = temp;
        }

        queue.Clear();
        for (int i = 0; i < array.Length; i++)
        {
            queue.Enqueue(array[i]);
        }

        return queue;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
