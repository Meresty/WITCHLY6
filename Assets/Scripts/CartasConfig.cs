using System.Collections.Generic;
using UnityEngine;

public static class CartasConfig
{
    public const string POCION_AMOR = "Poción de Amor";
    public const string ELIXIR_MUSA = "Elixir de la Musa";
    public const string VERITALIXIR = "Veritalixir";
    public const string ESENCIA_SOL = "Esencia del Sol";
    public const string REVITALIZANTE = "Revitalizante";
    public const string ESENCIA_SABIO = "Escencia del Sabio";
    public const string POCION_METAMORFICA = "Poción Metamórfica";
    public const string ELIXIR_SANACION = "Elixir de Sanación";
    public const string ELIXIR_SUBMARINO = "Elixir Submarino";
    public const string POCION_VALOR_INFALIBLE = "Poción de Valor Infalible";
    public const string BREBAJE_GUARDIAN = "Brebaje de Guardián";
    public const string ELIXIR_PURIFICACION = "Elixir de Purificación";
    public const string ESENCIA_LUNA = "Esencia de Luna";
    public const string POCION_RUPTURA = "Poción de Ruptura";
    public const string SERENIDAD_LIQUIDA = "Serenidad Líquida";


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

        //Poción de Valor Infalible
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

¿Podrías prepararme algo que me dé el valor suficiente para 
decir lo que pienso sin temblar?

— Cliente nervioso",
            pocionRequerida = POCION_VALOR_INFALIBLE,
            calidad = PotionQuality.Estandar,
            recompensaBase = 29
        });

        //Poción Revitalizante
        cartas.Add(new CartaData
        {
            numero = 2,
            etapa = CartaStage.Etapa1,
            titulo = "Cansancio que no se va",
            textoCarta =
@"Hechicera:

Trabajo todo el día en el mercado y, cuando llego a casa, ya no 
me queda energía ni para cenar. Los médicos dicen que estoy sano,
pero yo me siento agotado todo el tiempo.

Quiero volver a sentirme con fuerzas para atender a mis clientes 
sin arrastrar los pies.

— Mercader exhausto",
            pocionRequerida = REVITALIZANTE,
            calidad = PotionQuality.Estandar,
            recompensaBase = 29
        });

        //Poción Metamórfica
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

— Comerciante anónimo",
            pocionRequerida = POCION_METAMORFICA,
            calidad = PotionQuality.Estandar,
            recompensaBase = 27
        });

        //Elixir de Sanación
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

— Paciente desesperado",
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

No me pasa nada grave… pero todos los días siento un peso en el pecho.
No tengo ganas de salir, ni de ver a mis amigos, ni de hacer nada.

¿Existirá alguna poción que ayude a alejar esta tristeza y 
recordarme que todavía hay cosas bonitas?

— Vecina apagada",
            pocionRequerida = ESENCIA_SOL,
            calidad = PotionQuality.Estandar,
            recompensaBase = 32
        });

        //Serenidad Líquida
        cartas.Add(new CartaData
        {
            numero = 6,
            etapa = CartaStage.Etapa1,
            titulo = "Estrés al borde del colapso",
            textoCarta =
@"Bruja del bosque:

Entre el trabajo, la familia y las deudas siento que mi cabeza 
está a punto de explotar. Apenas duermo y siempre estoy tenso.

Necesito algo que me ayude a relajarme de verdad, sin dejarme 
medio dormido todo el día.

— Artesano estresado",
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

En unos días tengo un examen para entrar a la academia de magia teórica.
Estudio, pero mi mente se distrae y nada se me queda.

¿Podrías crear una poción que mejore mi concentración mientras estudio,
sin volverme un zombi?

— Aprendiz preocupado",
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
por un tiempo, podríamos intentar recuperarlo.

— Aventurero curioso",
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
Solo la usaré en una ''cata gratuita'' de bebidas.

— Dueño desconfiado",
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
Cuando por fin duermo, ya está amaneciendo.

¿Podrías prepararme algo que mejore mi sueño sin dejarme 
mareado al día siguiente?

— Panadero insomne",
            pocionRequerida = ESENCIA_LUNA,
            calidad = PotionQuality.Estandar,
            recompensaBase = 34
        });

        //Elixir de la Musa
        cartas.Add(new CartaData
        {
            numero = 11,
            etapa = CartaStage.Etapa1,
            titulo = "Inspiración bloqueada",
            textoCarta =
@"Hechicera del buzón:

Soy pintora y llevo semanas sin poder terminar un solo cuadro. 
Las ideas se me escapan y el lienzo sigue en blanco.

Dicen que tienes un elixir que despierta la creatividad. 
¿Podrías prepararme uno?

— Artista bloqueada",
            pocionRequerida = ELIXIR_MUSA,
            calidad = PotionQuality.Estandar,
            recompensaBase = 41
        });

        //Elixir de Purificación
        cartas.Add(new CartaData
        {
            numero = 12,
            etapa = CartaStage.Etapa1,
            titulo = "Maldición familiar",
            textoCarta =
@"Bruja:

En mi familia todos nacemos con una pequeña marca en el brazo 
que arde cuando estamos cerca de cierto lugar del bosque. 
Creemos que es una maldición antigua.

Busco una poción que ayude a romper maldiciones sin hacer daño 
a los que la bebemos.

— Heredero inquieto",
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

Mañana pelearé en un duelo público contra el mejor espadachín de la ciudad.
Sé defenderme, pero frente a tanta gente me tiemblan las manos.

Necesito una versión mejorada de esa poción que da valentía.
Si es posible, de calidad Plata. No puedo permitirme fallar.

— Retador nervioso",
            pocionRequerida = POCION_VALOR_INFALIBLE,
            calidad = PotionQuality.Plata,
            recompensaBase = 29
        });

        //Elixir de Sanación
        cartas.Add(new CartaData
        {
            numero = 14,
            etapa = CartaStage.Etapa2,
            titulo = "Curar a todo un escuadrón",
            textoCarta =
@"Bruja del buzón:

Formo parte de la guardia de la ciudad y varios compañeros
resultaron heridos en la última patrulla. 
Las curaciones comunes no son suficientes.

Busco un elixir de sanación más potente, de calidad Plata,
para repartirlo entre todos.

— Capitán preocupado",
            pocionRequerida = ELIXIR_SANACION,
            calidad = PotionQuality.Plata,
            recompensaBase = 43
        });

        //Esencia del Sol
        cartas.Add(new CartaData
        {
            numero = 15,
            etapa = CartaStage.Etapa2,
            titulo = "Taberna sin alegría",
            textoCarta =
@"Hechicera:

Mi taberna solía estar llena de risas, pero últimamente
los clientes llegan apagados y se van en silencio.
Ni la música ni el vino les anima.

Quisiera una Esencia del Sol de calidad Plata para
servirla en pequeñas dosis a mis clientes más tristes.

— Tabernera preocupada",
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

El alcalde no ha dormido bien en meses y eso está
afectando sus decisiones para el pueblo.
Los remedios normales ya no le hacen efecto.

Necesitamos una Esencia de Luna de la mejor calidad,
Oro, para que pueda descansar de verdad.

— Secretaria del ayuntamiento",
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
para más de cien estudiantes. Entre tanto papeleo 
ya no puedo concentrarme en lo importante.

Necesito una Escencia del Sabio de calidad Plata
para terminar el examen sin olvidar ningún detalle.

— Maestro abrumado",
            pocionRequerida = ESENCIA_SABIO,
            calidad = PotionQuality.Plata,
            recompensaBase = 39
        });

        //Serenidad Líquida
        cartas.Add(new CartaData
        {
            numero = 18,
            etapa = CartaStage.Etapa2,
            titulo = "Herrero al borde del colapso",
            textoCarta =
@"Bruja del bosque:

Tengo más encargos de armas de los que puedo manejar.
Trabajo día y noche, y si dejo de producir me arruino.

Quiero una Serenidad Líquida de calidad Plata
que me quite el estrés sin quitarme las ganas de trabajar.

— Herrero agotado",
            pocionRequerida = SERENIDAD_LIQUIDA,
            calidad = PotionQuality.Plata,
            recompensaBase = 36
        });

        //Poción de Amor
        cartas.Add(new CartaData
        {
            numero = 19,
            etapa = CartaStage.Etapa2,
            titulo = "Amor imposible en el palacio",
            textoCarta =
@"Hechicera confidencial:

Trabajo como sirviente en el palacio y estoy enamorado
de alguien que jamás se fijaría en mí.
No quiero obligar a nadie, solo llamar su atención
por una noche de baile.

Me hablaron de una Poción de Amor muy refinada.
Si existe una versión de calidad Oro, pagaré lo que haga falta.

— Sirviente enamorado",
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
@"Bruja del buzón:

Soy dramaturga y el estreno de mi nueva obra es en una semana.
Los actores ya están listos, pero el final simplemente no me convence.

Necesito un Elixir de la Musa de calidad Plata
para encontrar la escena perfecta antes del estreno.

— Autora desesperada",
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

Se celebrará un juicio público por un robo importante.
Hay varios sospechosos y todos mienten mejor que hablan.

Necesitamos un Veritalixir de calidad Oro
para asegurarnos de que al menos uno de ellos diga la verdad.

— Juez de la ciudad",
            pocionRequerida = VERITALIXIR,
            calidad = PotionQuality.Oro,
            recompensaBase = 33
        });

        //Brebaje de Guardián
        cartas.Add(new CartaData
        {
            numero = 22,
            etapa = CartaStage.Etapa2,
            titulo = "Casa embrujada en venta",
            textoCarta =
@"Bruja del bosque:

Estoy intentando vender una vieja mansión, pero los espíritus
que la habitan espantan a cualquier comprador que se acerca.

Necesito un Brebaje de Guardián de calidad Plata
para ahuyentar a los espíritus y por fin cerrar la venta.

— Agente inmobiliario frustrado",
            pocionRequerida = BREBAJE_GUARDIAN,
            calidad = PotionQuality.Plata,
            recompensaBase = 37
        });

        // Elixir de Purificación
        cartas.Add(new CartaData
        {
            numero = 23,
            etapa = CartaStage.Etapa2,
            titulo = "Reliquia maldita",
            textoCarta =
@"Bruja:

Compré una reliquia antigua en el mercado negro
y desde entonces escucho susurros cada noche.
Estoy seguro de que está maldita.

Busco un Elixir de Purificación de calidad Oro
para limpiar la reliquia sin destruirla.

— Coleccionista imprudente",
            pocionRequerida = ELIXIR_PURIFICACION,
            calidad = PotionQuality.Oro,
            recompensaBase = 45
        });

        //Elixir Submarino
        cartas.Add(new CartaData
        {
            numero = 24,
            etapa = CartaStage.Etapa2,
            titulo = "Exploración de cuevas marinas",
            textoCarta =
@"Saludos, hechicera:

Un equipo de exploradores y yo queremos mapear una red de cuevas
bajo el acantilado. El mar es agitado y el aire se acaba rápido.

Necesitamos Elixir Submarino de calidad Plata
para poder permanecer más tiempo bajo el agua.

— Explorador marino",
            pocionRequerida = ELIXIR_SUBMARINO,
            calidad = PotionQuality.Plata,
            recompensaBase = 34
        });

        //Poción de Ruptura
        cartas.Add(new CartaData
        {
            numero = 25,
            etapa = CartaStage.Etapa2,
            titulo = "Relación que hace daño",
            textoCarta =
@"Bruja del buzón:

Estoy atrapada en una relación que me hace daño,
pero cada vez que intento terminar, vuelvo a caer en lo mismo.

Me dijeron que existe una Poción de Ruptura
que ayuda a cortar lazos dañinos. 
Si puedes prepararla en calidad Oro, quiero estar segura
de que esta será la última vez.

— Corazón cansado",
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

Participaré en una carrera de resistencia en la montaña.
No quiero hacer trampa, solo asegurarme de no colapsar a mitad del camino.

Busco un Revitalizante de calidad Oro
que me ayude a mantener el ritmo hasta el final.

— Corredor entusiasmado",
            pocionRequerida = REVITALIZANTE,
            calidad = PotionQuality.Oro,
            recompensaBase = 29
        });

        //Poción Metamórfica
        cartas.Add(new CartaData
        {
            numero = 27,
            etapa = CartaStage.Etapa2,
            titulo = "Escapar de los paparazzi mágicos",
            textoCarta =
@"Bruja:

Desde que publiqué mi libro de hechizos, no puedo caminar
por la ciudad sin que todos me reconozcan y pidan autógrafos.

Quiero una Poción Metamórfica de calidad Plata
para poder salir un día normal sin ser perseguida.

— Autora famosa",
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

        //Poción de Ruptura
        cartas.Add(new CartaData
        {
            numero = 28,
            etapa = CartaStage.Especial,
            titulo = "Receta prohibida de ruptura",
            textoCarta =
@"Oferta especial:

En este pergamino se encuentra la receta completa
de una Poción de Ruptura, usada antiguamente por magas
que querían cortar lazos peligrosos.

Si compras esta carta, aprenderás a preparar la poción
y podrás aceptar encargos que la requieran.

— Vendedor misterioso",
            pocionRequerida = POCION_RUPTURA,
            calidad = PotionQuality.Estandar,
            recompensaBase = 66
        });

        //Brebaje de Guardián
        cartas.Add(new CartaData
        {
            numero = 29,
            etapa = CartaStage.Especial,
            titulo = "Manual del guardián espiritual",
            textoCarta =
@"Hechicera:

Este cuadernillo contiene invocaciones y notas antiguas
sobre cómo preparar un Brebaje de Guardián,
capaz de espantar espíritus molestos.

Comprando esta carta podrás elaborar la poción
y atender pedidos relacionados con casas embrujadas.

— Anticuario del mercado",
            pocionRequerida = BREBAJE_GUARDIAN,
            calidad = PotionQuality.Estandar,

            recompensaBase = 74
        });

        //Poción de Amor
        cartas.Add(new CartaData
        {
            numero = 30,
            etapa = CartaStage.Especial,
            titulo = "Tratado sobre vínculos del corazón",
            textoCarta =
@"Bruja del buzón:

Dentro de esta carta se esconde la fórmula
de una Poción de Amor cuidadosamente equilibrada.
No obliga a nadie, solo resalta sentimientos que ya existen.

Si decides comprarla, podrás preparar esta poción
y recibir encargos muy bien pagados relacionados con el amor.

— Archivista romántico",
            pocionRequerida = POCION_AMOR,
            calidad = PotionQuality.Estandar,
            recompensaBase = 88
        });

        return cartas;
    }
}
