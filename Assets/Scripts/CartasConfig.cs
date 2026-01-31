using System.Collections.Generic;
using UnityEngine;

public static class CartasConfig
{
    public const string POCION_AMOR = "Pocion de Amor";
    public const string ELIXIR_MUSA = "Elixir de la Musa";
    public const string VERITALIXIR = "Veritalixir";
    public const string ESENCIA_SOL = "Esencia del Sol";
    public const string REVITALIZANTE = "Revitalizante";
    public const string ESENCIA_SABIO = "Escencia del Sabio";
    public const string POCION_METAMORFICA = "Pocion Metamorfica";
    public const string ELIXIR_SANACION = "Elixir de Sanacion";
    public const string ELIXIR_SUBMARINO = "Elixir Submarino";
    public const string POCION_VALOR_INFALIBLE = "Pocion de Valor Infalible";
    public const string BREBAJE_GUARDIAN = "Brebaje de Guardion";
    public const string ELIXIR_PURIFICACION = "Elixir de Purificacion";
    public const string ESENCIA_LUNA = "Esencia de Luna";
    public const string POCION_RUPTURA = "Pocion de Ruptura";
    public const string SERENIDAD_LIQUIDA = "Serenidad Liquida";


    public static readonly string[] POCIONES_INTRO_ETAPA1 = new[]
    {
        POCION_VALOR_INFALIBLE,
        REVITALIZANTE,
        POCION_METAMORFICA
    };


    public static readonly string[] POCIONES_DESBLOQUEADAS_POR_CARTAS_ESPECIALES = new[]
    {
        POCION_RUPTURA,
        POCION_AMOR,
        BREBAJE_GUARDIAN
    };


    public static List<CartaData> CrearCartasEtapa1()
    {
        var cartas = new List<CartaData>();

        //Valor Infalible
        cartas.Add(new CartaData
        {
            numero = 1,
            etapa = CartaStage.Etapa1,
            titulo = "Necesito valor para hablar",
            textoCarta =
@"Brujita del buzón:

Mañana tengo que hablar con mi jefe para pedirle un ascenso, 
pero solo de pensarlo me pongo nervioso. Siempre me quedo 
callado aunque sepa que merezco más.

Podrías prepararme algo que me de el valor suficiente para 
decir lo que pienso sin temblar?

- Cliente nervioso",
            pocionRequerida = POCION_VALOR_INFALIBLE,
            calidad = PotionQuality.Estandar,
            recompensaBase = 29
        });

        //Poci�n Revitalizante
        cartas.Add(new CartaData
        {
            numero = 2,
            etapa = CartaStage.Etapa1,
            titulo = "Cansancio que no se va",
            textoCarta =
@"Hechicera:

Trabajo todo el día en el mercado y, cuando llego a casa, ya no 
me queda energ�a ni para cenar. Los m�dicos dicen que estoy sano,
pero yo me siento agotado todo el tiempo.

Quiero volver a sentirme con fuerzas para atender a mis clientes 
sin arrastrar los pies.

 - Mercader exhausto",
            pocionRequerida = REVITALIZANTE,
            calidad = PotionQuality.Estandar,
            recompensaBase = 29
        });

        //Poci�n Metam�rfica
        cartas.Add(new CartaData
        {
            numero = 3,
            etapa = CartaStage.Etapa1,
            titulo = "Una noche con otra cara",
            textoCarta =
@"Bruja:

El próximo festival de máscaras se acerca y quiero ir sin que nadie 
me reconozca. No quiero problemas con cierta gente que me debe dinero.

Necesito algo que me permita cambiar mi apariencia física por un día, 
solo para disfrutar del festival en paz.

- Comerciante anónimo",
            pocionRequerida = POCION_METAMORFICA,
            calidad = PotionQuality.Estandar,
            recompensaBase = 27
        });

        //Elixir de Sanaci�n
        cartas.Add(new CartaData
        {
            numero = 4,
            etapa = CartaStage.Etapa1,
            titulo = "Enfermedad que no cede",
            textoCarta =
@"Buenas noches:

Desde hace semanas tengo una fiebre que va y viene. 
Las hierbas comunes ya no me hacen efecto y el curandero del pueblo 
dice que es algo ''persistente''.

Me hablaron de tus pociones. ¿Tienes alguna que ayude a curar 
enfermedades que simplemente no se quieren ir?

- Paciente desesperado",
            pocionRequerida = ELIXIR_SANACION,
            calidad = PotionQuality.Estandar,
            recompensaBase = 43
        });

        //Esencia del Sol
        cartas.Add(new CartaData
        {
            numero = 5,
            etapa = CartaStage.Etapa1,
            titulo = "Tristeza sin motivo",
            textoCarta =
@"Hechicera:

No me pasa nada grave, pero todos los días siento un peso en el pecho.
No tengo ganas de salir, ni de ver a mis amigos, ni de hacer nada.

¿Existirá alguna poción que ayude a alejar esta tristeza y 
recordarme que todavía hay cosas bonitas?

- Vecina apagada",
            pocionRequerida = ESENCIA_SOL,
            calidad = PotionQuality.Estandar,
            recompensaBase = 32
        });

        //Serenidad L�quida
        cartas.Add(new CartaData
        {
            numero = 6,
            etapa = CartaStage.Etapa1,
            titulo = "Estr�s al borde del colapso",
            textoCarta =
@"Bruja del bosque:

Entre el trabajo, la familia y las deudas siento que mi cabeza 
está a punto de explotar. Apenas duermo y siempre estoy tenso.

Necesito algo que me ayude a relajarme de verdad, sin dejarme 
medio dormido todo el d�a.

- Artesano estresado",
            pocionRequerida = SERENIDAD_LIQUIDA,
            calidad = PotionQuality.Estandar,
            recompensaBase = 36
        });

        //Escencia del Sabio
        cartas.Add(new CartaData
        {
            numero = 7,
            etapa = CartaStage.Etapa1,
            titulo = "Examen imposible",
            textoCarta =
@"Bruja:

En unos días tengo un examen para entrar a la academia de magia te�rica.
Estudio, pero mi mente se distrae y nada se me queda.

¿Podrías crear una poci�n que mejore mi concentraci�n mientras estudio,
sin volverme un zombi?

- Aprendiz preocupado",
            pocionRequerida = ESENCIA_SABIO,
            calidad = PotionQuality.Estandar,
            recompensaBase = 39
        });

        //Elixir Submarino
        cartas.Add(new CartaData
        {
            numero = 8,
            etapa = CartaStage.Etapa1,
            titulo = "Tesoro bajo el lago",
            textoCarta =
@"Saludos:

Se rumora que en el fondo del lago hay un cofre antiguo. 
Ya intentamos bucear varias veces, pero el aire no alcanza 
y el agua es muy fría.

Si tuvieras algo que nos permitiera respirar bajo el agua 
por un tiempo, podr�amos intentar recuperarlo.

- Aventurero curioso",
            pocionRequerida = ELIXIR_SUBMARINO,
            calidad = PotionQuality.Estandar,
            recompensaBase = 34
        });

        //Veritalixir
        cartas.Add(new CartaData
        {
            numero = 9,
            etapa = CartaStage.Etapa1,
            titulo = "Sospechas en la taberna",
            textoCarta =
@"Hechicera:

Creo que uno de mis empleados está robando de la caja, 
pero nunca lo puedo agarrar en el momento.

¿Tienes alguna poción que obligue a decir la verdad durante una charla?
Solo la usaré en una cata gratuita de bebidas :) .

� Due�o desconfiado",
            pocionRequerida = VERITALIXIR,
            calidad = PotionQuality.Estandar,
            recompensaBase = 33
        });

        //Esencia de Luna
        cartas.Add(new CartaData
        {
            numero = 10,
            etapa = CartaStage.Etapa1,
            titulo = "No puedo dormir",
            textoCarta =
@"Bruja:

Cada noche doy vueltas y vueltas en la cama. 
Cuando por fin duermo, ya est� amaneciendo.

�Podr�as prepararme algo que mejore mi sue�o sin dejarme 
mareado al d�a siguiente?

� Panadero insomne",
            pocionRequerida = ESENCIA_LUNA,
            calidad = PotionQuality.Estandar,
            recompensaBase = 34
        });

        //Elixir de la Musa
        cartas.Add(new CartaData
        {
            numero = 11,
            etapa = CartaStage.Etapa1,
            titulo = "Inspiraci�n bloqueada",
            textoCarta =
@"Hechicera del buz�n:

Soy pintora y llevo semanas sin poder terminar un solo cuadro. 
Las ideas se me escapan y el lienzo sigue en blanco.

Dicen que tienes un elixir que despierta la creatividad. 
�Podr�as prepararme uno?

� Artista bloqueada",
            pocionRequerida = ELIXIR_MUSA,
            calidad = PotionQuality.Estandar,
            recompensaBase = 41
        });

        //Elixir de Purificaci�n
        cartas.Add(new CartaData
        {
            numero = 12,
            etapa = CartaStage.Etapa1,
            titulo = "Maldici�n familiar",
            textoCarta =
@"Bruja:

En mi familia todos nacemos con una peque�a marca en el brazo 
que arde cuando estamos cerca de cierto lugar del bosque. 
Creemos que es una maldici�n antigua.

Busco una poci�n que ayude a romper maldiciones sin hacer da�o 
a los que la bebemos.

� Heredero inquieto",
            pocionRequerida = ELIXIR_PURIFICACION,
            calidad = PotionQuality.Estandar,
            recompensaBase = 45
        });

        return cartas;
    }


    //etapa 2 

    public static List<CartaData> CrearCartasEtapa2()
    {
        var cartas = new List<CartaData>();

        //Valor Infalible
        cartas.Add(new CartaData
        {
            numero = 13,
            etapa = CartaStage.Etapa2,
            titulo = "Duelo en la plaza central",
            textoCarta =
@"Hechicera:

Ma�ana pelear� en un duelo p�blico contra el mejor espadach�n de la ciudad.
S� defenderme, pero frente a tanta gente me tiemblan las manos.

Necesito una versi�n mejorada de esa poci�n que da valent�a.
Si es posible, de calidad Plata. No puedo permitirme fallar.

� Retador nervioso",
            pocionRequerida = POCION_VALOR_INFALIBLE,
            calidad = PotionQuality.Plata,
            recompensaBase = 29
        });

        //Elixir de Sanaci�n
        cartas.Add(new CartaData
        {
            numero = 14,
            etapa = CartaStage.Etapa2,
            titulo = "Curar a todo un escuadr�n",
            textoCarta =
@"Bruja del buz�n:

Formo parte de la guardia de la ciudad y varios compa�eros
resultaron heridos en la �ltima patrulla. 
Las curaciones comunes no son suficientes.

Busco un elixir de sanaci�n m�s potente, de calidad Plata,
para repartirlo entre todos.

� Capit�n preocupado",
            pocionRequerida = ELIXIR_SANACION,
            calidad = PotionQuality.Plata,
            recompensaBase = 43
        });

        //Esencia del Sol
        cartas.Add(new CartaData
        {
            numero = 15,
            etapa = CartaStage.Etapa2,
            titulo = "Taberna sin alegr�a",
            textoCarta =
@"Hechicera:

Mi taberna sol�a estar llena de risas, pero �ltimamente
los clientes llegan apagados y se van en silencio.
Ni la m�sica ni el vino les anima.

Quisiera una Esencia del Sol de calidad Plata para
servirla en peque�as dosis a mis clientes m�s tristes.

� Tabernera preocupada",
            pocionRequerida = ESENCIA_SOL,
            calidad = PotionQuality.Plata,
            recompensaBase = 32
        });

        //Esencia de Luna
        cartas.Add(new CartaData
        {
            numero = 16,
            etapa = CartaStage.Etapa2,
            titulo = "Insomnio del alcalde",
            textoCarta =
@"Bruja:

El alcalde no ha dormido bien en meses y eso est�
afectando sus decisiones para el pueblo.
Los remedios normales ya no le hacen efecto.

Necesitamos una Esencia de Luna de la mejor calidad,
Oro, para que pueda descansar de verdad.

� Secretaria del ayuntamiento",
            pocionRequerida = ESENCIA_LUNA,
            calidad = PotionQuality.Oro,
            recompensaBase = 34
        });

        //Escencia del Sabio
        cartas.Add(new CartaData
        {
            numero = 17,
            etapa = CartaStage.Etapa2,
            titulo = "Biblioteca saturada",
            textoCarta =
@"Brujita:

Soy maestro en la academia y debo preparar un examen
para m�s de cien estudiantes. Entre tanto papeleo 
ya no puedo concentrarme en lo importante.

Necesito una Escencia del Sabio de calidad Plata
para terminar el examen sin olvidar ning�n detalle.

� Maestro abrumado",
            pocionRequerida = ESENCIA_SABIO,
            calidad = PotionQuality.Plata,
            recompensaBase = 39
        });

        //Serenidad L�quida
        cartas.Add(new CartaData
        {
            numero = 18,
            etapa = CartaStage.Etapa2,
            titulo = "Herrero al borde del colapso",
            textoCarta =
@"Bruja del bosque:

Tengo m�s encargos de armas de los que puedo manejar.
Trabajo d�a y noche, y si dejo de producir me arruino.

Quiero una Serenidad L�quida de calidad Plata
que me quite el estr�s sin quitarme las ganas de trabajar.

� Herrero agotado",
            pocionRequerida = SERENIDAD_LIQUIDA,
            calidad = PotionQuality.Plata,
            recompensaBase = 36
        });

        //Poci�n de Amor
        cartas.Add(new CartaData
        {
            numero = 19,
            etapa = CartaStage.Etapa2,
            titulo = "Amor imposible en el palacio",
            textoCarta =
@"Hechicera confidencial:

Trabajo como sirviente en el palacio y estoy enamorado
de alguien que jam�s se fijar�a en m�.
No quiero obligar a nadie, solo llamar su atenci�n
por una noche de baile.

Me hablaron de una Poci�n de Amor muy refinada.
Si existe una versi�n de calidad Oro, pagar� lo que haga falta.

� Sirviente enamorado",
            pocionRequerida = POCION_AMOR,
            calidad = PotionQuality.Oro,
            recompensaBase = 44
        });

        //Elixir de la Musa
        cartas.Add(new CartaData
        {
            numero = 20,
            etapa = CartaStage.Etapa2,
            titulo = "Obra de teatro en crisis",
            textoCarta =
@"Bruja del buz�n:

Soy dramaturga y el estreno de mi nueva obra es en una semana.
Los actores ya est�n listos, pero el final simplemente no me convence.

Necesito un Elixir de la Musa de calidad Plata
para encontrar la escena perfecta antes del estreno.

� Autora desesperada",
            pocionRequerida = ELIXIR_MUSA,
            calidad = PotionQuality.Plata,
            recompensaBase = 41
        });

        //Veritalixir
        cartas.Add(new CartaData
        {
            numero = 21,
            etapa = CartaStage.Etapa2,
            titulo = "Juicio en la plaza",
            textoCarta =
@"Hechicera:

Se celebrar� un juicio p�blico por un robo importante.
Hay varios sospechosos y todos mienten mejor que hablan.

Necesitamos un Veritalixir de calidad Oro
para asegurarnos de que al menos uno de ellos diga la verdad.

� Juez de la ciudad",
            pocionRequerida = VERITALIXIR,
            calidad = PotionQuality.Oro,
            recompensaBase = 33
        });

        //Brebaje de Guardi�n
        cartas.Add(new CartaData
        {
            numero = 22,
            etapa = CartaStage.Etapa2,
            titulo = "Casa embrujada en venta",
            textoCarta =
@"Bruja del bosque:

Estoy intentando vender una vieja mansi�n, pero los esp�ritus
que la habitan espantan a cualquier comprador que se acerca.

Necesito un Brebaje de Guardi�n de calidad Plata
para ahuyentar a los esp�ritus y por fin cerrar la venta.

� Agente inmobiliario frustrado",
            pocionRequerida = BREBAJE_GUARDIAN,
            calidad = PotionQuality.Plata,
            recompensaBase = 37
        });

        // Elixir de Purificaci�n
        cartas.Add(new CartaData
        {
            numero = 23,
            etapa = CartaStage.Etapa2,
            titulo = "Reliquia maldita",
            textoCarta =
@"Bruja:

Compr� una reliquia antigua en el mercado negro
y desde entonces escucho susurros cada noche.
Estoy seguro de que est� maldita.

Busco un Elixir de Purificaci�n de calidad Oro
para limpiar la reliquia sin destruirla.

� Coleccionista imprudente",
            pocionRequerida = ELIXIR_PURIFICACION,
            calidad = PotionQuality.Oro,
            recompensaBase = 45
        });

        //Elixir Submarino
        cartas.Add(new CartaData
        {
            numero = 24,
            etapa = CartaStage.Etapa2,
            titulo = "Exploraci�n de cuevas marinas",
            textoCarta =
@"Saludos, hechicera:

Un equipo de exploradores y yo queremos mapear una red de cuevas
bajo el acantilado. El mar es agitado y el aire se acaba r�pido.

Necesitamos Elixir Submarino de calidad Plata
para poder permanecer m�s tiempo bajo el agua.

� Explorador marino",
            pocionRequerida = ELIXIR_SUBMARINO,
            calidad = PotionQuality.Plata,
            recompensaBase = 34
        });

        //Poci�n de Ruptura
        cartas.Add(new CartaData
        {
            numero = 25,
            etapa = CartaStage.Etapa2,
            titulo = "Relaci�n que hace da�o",
            textoCarta =
@"Bruja del buz�n:

Estoy atrapada en una relaci�n que me hace da�o,
pero cada vez que intento terminar, vuelvo a caer en lo mismo.

Me dijeron que existe una Poci�n de Ruptura
que ayuda a cortar lazos da�inos. 
Si puedes prepararla en calidad Oro, quiero estar segura
de que esta ser� la �ltima vez.

� Coraz�n cansado",
            pocionRequerida = POCION_RUPTURA,
            calidad = PotionQuality.Oro,
            recompensaBase = 33
        });

        //Revitalizante 
        cartas.Add(new CartaData
        {
            numero = 26,
            etapa = CartaStage.Etapa2,
            titulo = "Carrera de resistencia",
            textoCarta =
@"Hechicera:

Participar� en una carrera de resistencia en la monta�a.
No quiero hacer trampa, solo asegurarme de no colapsar a mitad del camino.

Busco un Revitalizante de calidad Oro
que me ayude a mantener el ritmo hasta el final.

� Corredor entusiasmado",
            pocionRequerida = REVITALIZANTE,
            calidad = PotionQuality.Oro,
            recompensaBase = 29
        });

        //Poci�n Metam�rfica
        cartas.Add(new CartaData
        {
            numero = 27,
            etapa = CartaStage.Etapa2,
            titulo = "Escapar de los paparazzi m�gicos",
            textoCarta =
@"Bruja:

Desde que publiqu� mi libro de hechizos, no puedo caminar
por la ciudad sin que todos me reconozcan y pidan aut�grafos.

Quiero una Poci�n Metam�rfica de calidad Plata
para poder salir un d�a normal sin ser perseguida.

� Autora famosa",
            pocionRequerida = POCION_METAMORFICA,
            calidad = PotionQuality.Plata,
            recompensaBase = 27
        });

        return cartas;
    }


    //  especiales


    public static List<CartaData> CrearCartasEspeciales()
    {
        var cartas = new List<CartaData>();

        //Poci�n de Ruptura
        cartas.Add(new CartaData
        {
            numero = 28,
            etapa = CartaStage.Especial,
            titulo = "Receta prohibida de ruptura",
            textoCarta =
@"Oferta especial:

En este pergamino se encuentra la receta completa
de una Poci�n de Ruptura, usada antiguamente por magas
que quer�an cortar lazos peligrosos.

Si compras esta carta, aprender�s a preparar la poci�n
y podr�s aceptar encargos que la requieran.

� Vendedor misterioso",
            pocionRequerida = POCION_RUPTURA,
            calidad = PotionQuality.Estandar,
            recompensaBase = 66
        });

        //Brebaje de Guardi�n
        cartas.Add(new CartaData
        {
            numero = 29,
            etapa = CartaStage.Especial,
            titulo = "Manual del guardi�n espiritual",
            textoCarta =
@"Hechicera:

Este cuadernillo contiene invocaciones y notas antiguas
sobre c�mo preparar un Brebaje de Guardi�n,
capaz de espantar esp�ritus molestos.

Comprando esta carta podr�s elaborar la poci�n
y atender pedidos relacionados con casas embrujadas.

� Anticuario del mercado",
            pocionRequerida = BREBAJE_GUARDIAN,
            calidad = PotionQuality.Estandar,

            recompensaBase = 74
        });

        //Poci�n de Amor
        cartas.Add(new CartaData
        {
            numero = 30,
            etapa = CartaStage.Especial,
            titulo = "Tratado sobre v�nculos del coraz�n",
            textoCarta =
@"Bruja del buz�n:

Dentro de esta carta se esconde la f�rmula
de una Poci�n de Amor cuidadosamente equilibrada.
No obliga a nadie, solo resalta sentimientos que ya existen.

Si decides comprarla, podr�s preparar esta poci�n
y recibir encargos muy bien pagados relacionados con el amor.

� Archivista rom�ntico",
            pocionRequerida = POCION_AMOR,
            calidad = PotionQuality.Estandar,
            recompensaBase = 88
        });

        return cartas;
    }
}
