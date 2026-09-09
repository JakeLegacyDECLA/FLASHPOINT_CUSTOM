# UNIDAD 6 SÉPTIMO RESCATE — Una simulación del Estudio 5.5.4

> *"Los ataques, las pérdidas, todo recae en los hombros de los Seis en sus últimos momentos."*

El septimo rescate es una simulación multi-agente donde Los Seis deciden por sí mismos,
turno a turno, gracias a un algoritmo de coordinación autónomo
que reevalúa la situación de la casa y reparte objetivos sin intervención humana. Lo
único que hace quien corre la simulación es **lanzar la partida** (con una semilla, un
número de agentes, una estrategia) y **observar** cómo se desenvuelve.

Las mecánicas están basadas en las reglas de *Flash Point Fire Rescue*, reambientadas
como una operación de rescate durante un brote de infección: el fuego, el humo y las
explosiones del juego de mesa original se convierten aquí en **horda**, **infección
latente** y **brotes masivos**; los bomberos se convierten en **Los Seis**, un colectivo
de hermanos que se mueve y decide como un solo organismo.

---

## Premisa

Nativos de las minas de **Site Nueve**, en Alabama, Los Seis nunca necesitaron palabras
para entenderse entre sí. Comparten los mismos tics, la misma postura, la misma
sintonía absoluta — perfeccionada desde el vientre materno.

Tras abastecerse de medicina y gasolina en **Bowling Green**, **Cinco** detecta gritos de
socorro distantes, en las colinas de Kentucky. Sin diálogo, sin votación, el grupo gira
la camioneta hacia el foco del conflicto.

*"Los ruidos vienen de ahí."*

La casa en la colina está a oscuras. El rastro de arañazos en la puerta derribada lo
confirma: **nunca vieron un infectado, pero saben que estuvieron ahí.**

Este es el séptimo rescate simulado de la temporada. No será el último.

---

## Los Seis (los agentes)

Seis hermanos idénticos, distinguibles solo por el color de su uniforme, cada uno un
**agente autónomo** que ejecuta su propio turno sin esperar instrucciones externas.
Ninguno recibe órdenes de quien observa la simulación; cada uno puede, por decisión
propia:

- **Avanzar** por la casa, sorteando muebles caídos y puertas atrancadas.
- **Forzar** una puerta o derribar un muro dañado (a costo de tiempo y ruido).
- **Contener** una zona infestada, reduciendo la agresividad de la horda antes de que
  se le escape de las manos.
- **Cargar a un superviviente** y sacarlo de la casa — la única prioridad que ningún
  agente negocia, ni siquiera con el resto del colectivo.

Cada turno, el agente activo recibe **4 puntos de acción** para gastar según su objetivo
asignado. Cuando uno de ellos es alcanzado por la horda, cae — y si en ese momento
cargaba a un superviviente, lo pierde ahí mismo. El agente caído es reubicado de
inmediato en el punto de extracción más cercano para recuperarse; no vuelve a perder
turnos futuros por ello, pero el tiempo ya está gastado.

---

## La casa (el entorno simulado)

Una mansión de una plantas en lo alto de una colina, con **4 puntos de extracción**
(las mismas puertas por las que entró la camioneta) y habitaciones conectadas por
puertas y muros de distinta resistencia. Cada muro dañado dos veces cede por completo,
y cada golpe suma al contador de **colapso estructural** — la casa entera puede
derrumbarse antes de que termine el rescate.

### Lo que puede haber en cada habitación

| Señal | Qué significa |
|---|---|
| **Grito sin identificar** | Puede ser un superviviente real o una falsa alarma — solo se sabe al llegar. |
| **Susurro / murmullo** | Presencia latente de infección: si nadie la atiende, puede convertirse en horda activa. |
| **Horda activa** | Zona tomada; ataca a cualquier agente que se quede parado ahí y amenaza con propagarse a las habitaciones contiguas. |
| **Brote masivo** | Una celda de horda que vuelve a ser "detonada": se expande en las 4 direcciones, dañando muros y contagiando lo que encuentra a su paso — el equivalente a una explosión. |

Cada turno, el entorno reacciona por su cuenta: algo nuevo se agita en algún punto de la
casa (una tirada de dado decide dónde), y los focos de auxilio pendientes se van
reponiendo hasta haber siempre 3 activos. Esta parte de la simulación no depende de
Los Seis ni de quien observa — es el "clima" autónomo del entorno.

---

## Ciclo de un turno simulado

1. **Revisión de estado**: ¿ya se perdieron 4 supervivientes, o la casa acumuló 24
   puntos de colapso estructural? Si es así, la simulación termina en derrota. ¿Ya se
   rescataron 7? Victoria. En cualquiera de los dos casos, la corrida se detiene sola.
2. **Coordinación silenciosa**: sin intervención externa, el colectivo reevalúa
   internamente quién va a dónde (ver siguiente sección).
3. **Acción del agente**: el rescatista en turno gasta sus puntos de acción moviéndose,
   forzando accesos, conteniendo la horda o resolviendo el foco al que llegó — todo
   decidido por su propia lógica, no por un input externo.
4. **La casa reacciona**: la infección avanza un paso en algún punto del tablero.
5. **Reposición**: el entorno asegura nuevos focos de auxilio hasta tener 3 activos.

Este ciclo se repite automáticamente hasta que la simulación alcanza una condición de
fin, o hasta un límite de turnos configurado de antemano para la corrida.

---

## Coordinación silenciosa (el algoritmo detrás de la sintonía)

Los Seis no necesitan hablar porque, en el fondo, todos calculan lo mismo. Este es el
mecanismo autónomo que reemplaza cualquier intervención de un jugador humano: cada
turno, antes de que un agente actúe, el colectivo reevalúa internamente sus objetivos.

1. **Focos de auxilio**: cada grito sin identificar es un foco urgente por sí solo.
2. **Puntos de quiebre**: se rastrea, desde cada superviviente, el camino más corto
   hasta la horda que podría alcanzarlo — ese es el punto exacto donde cortarle el
   paso antes de que sea tarde. Cuanto más cerca está esa amenaza del superviviente,
   más urgente se vuelve.
3. **Eslabones débiles**: dentro de una horda ya extendida, se identifica el punto
   medio de la cadena — romperla ahí la parte en dos focos menores, en vez de solo
   raspar el borde.
4. **Limpieza general**: cualquier zona infestada que ningún otro foco esté cubriendo
   también entra en la lista, aunque sea con menor urgencia, para que la casa no se
   pudra sin control mientras el colectivo se concentra en lo "importante".

Con todos los focos sobre la mesa, cada uno se ordena por urgencia y se combina con la
distancia real que le toma a cada agente llegar — un foco muy urgente puede ganarle a
uno más cercano pero menos grave. El colectivo resuelve internamente, sin negociación
externa, la combinación de asignaciones que minimiza el costo total: cada agente libre
termina con un objetivo, y ninguno se pisa el terreno con otro.

Si la situación se sale de control — la casa empieza a colapsar o la infección crece
más allá de un umbral crítico — el colectivo entero **suelta lo que estaba haciendo**
(salvo el agente que ya carga a un superviviente, o el que está caído) y vuelve a
repartirse desde cero, con la nueva realidad del tablero sobre la mesa.


Acceso al repositorio que contiene el sistema multiagente: [Flashpoint_Multiagente](https://github.com/RodrigoHDev/Flashpoint_Multiagentes)

---

## Condiciones de fin de la simulación

| Resultado | Condición |
|---|---|
| 🟢 **VICTORIA** | 7 supervivientes rescatados. *"Las hordas no pueden enfrentar la coordinación de los Seis en acción."* |
| 🔴 **DERROTA** | 4 supervivientes perdidos, **o** la casa acumula 24 puntos de colapso estructural. |

---

## Panel de observación

Quien corre la simulación no interactúa con Los Seis — solo observa su avance a través
de un panel que muestra, en tiempo real:

- **Turno** actual de la corrida.
- **Rescatados**: supervivientes ya puestos a salvo por el colectivo.
- **Infectados**: focos ya perdidos ante la horda.
- **Estado**: nivel de colapso/infección acumulado en la casa.
- El plano de la mansión, con Los Seis moviéndose entre habitaciones, focos de auxilio
  marcados y las zonas de extracción resaltadas.

Una misma simulación puede correrse muchas veces con distintas semillas para observar
qué tan consistentemente el colectivo logra sacar a los 7 supervivientes antes de que
la casa colapse, sin que nadie desde afuera mueva una sola pieza.

---

*Estudio 5.5.4 — este es el séptimo rescate simulado documentado. La casa en la colina
de Kentucky no será la última.*