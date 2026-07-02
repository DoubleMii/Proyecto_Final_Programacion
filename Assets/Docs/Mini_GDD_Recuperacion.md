# Mini GDD - Recuperacion individual

## Concepto

Juego 3D de exploracion y supervivencia ligera. El jugador recorre un escenario con zonas de peligro, enemigos con patrullas y menus de pausa/guardado. El objetivo de la recuperacion es que los sistemas creados existan de forma visible en la build, no solo como scripts sueltos.

## Gameplay

- El jugador se mueve por el nivel evitando enemigos.
- Los NPC detectan al jugador por cercania, campo de vision y raycast.
- Si el jugador es visto, el NPC entra en alerta, persigue o ataca segun su rol.
- Si el NPC pierde al jugador, busca en la ultima posicion conocida antes de volver a patrullar.
- La partida se puede guardar/cargar en varios slots JSON.
- La build puede cerrarse desde el menu de pausa.

## NPCs activos

Hay 3 roles diferenciados:

- `Guard`: patrulla equilibrada, sospecha, persigue y ataca a corta distancia.
- `Hunter`: mas rapido, mas agresivo y con busqueda mas larga al perder al jugador.
- `Sentinel`: detector de largo alcance, mas lento, ataca/mantiene distancia y vigila una zona.

La build incluye un integrador runtime que revisa la escena al cargar. Si hay NPCs desactivados, los activa y les asigna roles. Si faltan NPCs, crea enemigos de respaldo sobre el NavMesh para asegurar que minimo 3 comportamientos esten visibles.

## Feedback audiovisual

- Materiales por rol: cada tipo de NPC recibe color propio para distinguirlo en juego.
- Audio: los NPC emiten un tono espacial al entrar en alerta o atacar.
- VFX: alerta, polvo y golpe tienen particulas de respaldo generadas en runtime si no hay prefabs asignados.
- Animacion: los personajes usan `Animator` si existe; si no, tienen movimiento procedural visible para evitar objetos completamente estaticos.

## Persistencia

El sistema Save/Load guarda datos en JSON:

- posicion y rotacion del jugador;
- vida, stamina, nivel y oro;
- inventario como lista de nombres;
- estadisticas de partida;
- ajustes de audio;
- slot, fecha y metadatos.

Hay 3 slots independientes y un boton de reset/borrado de slot funcional.

## Controles esperados

- `Escape`: pausa/reanuda.
- Menu de pausa: continuar, reiniciar y salir.
- Menu de guardado: seleccionar slot, guardar, cargar, nueva partida y reset/borrar slot.

## Criterios de verificacion

1. La escena principal esta incluida en Build Settings.
2. Al ejecutar, aparecen varios NPCs activos.
3. Los NPCs patrullan, detectan por vision/raycast, persiguen, buscan y vuelven a patrullar.
4. Al entrar en alerta se ve VFX y se oye audio.
5. Los enemigos tienen colores/materiales diferenciados.
6. Guardar/cargar cambia posicion y datos del jugador.
7. Reset de slot elimina el guardado y reinicia datos.
8. El boton salir cierra la build o detiene Play Mode en editor.
