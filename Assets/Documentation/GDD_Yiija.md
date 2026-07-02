# GDD breve - Yiija

## Portada

**Nombre del juego:** Yiija  
**Genero:** aventura de exploracion y sigilo en 3D.  
**Plataforma objetivo:** PC.  
**Version:** recuperacion final.

Yiija es una aventura en tercera persona ambientada en un escenario oscuro donde el jugador debe explorar, evitar enemigos y alcanzar el objetivo final sin ser detectado. El juego combina navegacion 3D, IA con NavMesh, deteccion por raycast, audio espacial, guardado en JSON y UI de pausa/guardado para construir una experiencia jugable de inicio a fin.

## Justificacion del genero

El genero elegido es aventura de exploracion y sigilo porque encaja con los contenidos tecnicos trabajados: movimiento del jugador en un entorno 3D, NPCs con NavMesh, estados de IA, deteccion por linea de vision, audio 3D, UI, persistencia y condicion de victoria/derrota. El alcance es razonable para una escena unica jugable y permite demostrar varios sistemas sin depender de muchos niveles.

## Premisa

El jugador despierta en un entorno hostil y debe escapar explorando el escenario, evitando enemigos y usando la informacion del entorno para llegar al punto de victoria. Si un enemigo detecta y alcanza al jugador, se activa la derrota y se reinicia el ciclo de juego.

## Bucle de juego

1. El jugador aparece en la escena principal.
2. Explora el escenario y localiza el camino seguro.
3. Los NPCs patrullan, deambulan o persiguen usando NavMesh.
4. Si el jugador entra en el campo de vision o proximidad de un NPC, se activa la persecucion.
5. El jugador puede pausar, guardar/cargar y continuar.
6. La partida termina con victoria al alcanzar el objetivo o derrota si el enemigo atrapa al jugador.

## Mecanicas principales

- Movimiento libre por escenario 3D.
- Deteccion enemiga por proximidad y raycast.
- NPCs con estados diferenciados: patrulla, espera, persecucion, ataque, huida o reposicionamiento segun el tipo.
- Sistema de guardado/carga por JSON con multiples slots.
- UI completa con HUD, pausa, menu de guardado y estado de victoria.
- Audio 3D para fuentes diegeticas y sistema de pasos por superficie.
- Feedback visual y de camara en situaciones clave.

## Personajes y NPCs

**Jugador:** personaje controlado por el usuario. Sus datos persistentes incluyen posicion, rotacion, vida, stamina, nivel, oro, inventario y configuracion de audio.

**Guardia patrullero:** recorre waypoints y pasa a persecucion si detecta al jugador. Estados principales: Patrol, Chase, Attack.

**Perseguidor:** busca activamente al jugador desde el inicio. Estados principales: Chase, Attack.

**Roamer:** permanece o deambula por una zona y reacciona al jugador si entra en su radio. Estados principales: Idle, Chase, Flee.

## Nivel

La escena principal es `Assets/Scenes/01_Main_Level.unity`. Incluye el espacio jugable, NavMesh, jugador, NPCs, UI, sistema de persistencia, audio y condicion de victoria/derrota. La escena esta configurada como escena activa de build.

## Estetica visual y audio

La estetica busca una atmosfera oscura y de tension, apoyada por materiales diferenciados, fuego/particulas, luces de ambiente y audio espacial. El audio se organiza mediante Audio Mixer con grupos separados para Master, musica y efectos, de forma que se puedan ajustar volumenes y aplicar efectos DSP.

## Condiciones de victoria y derrota

**Victoria:** el jugador alcanza el objetivo final de la escena.  
**Derrota:** un NPC alcanza al jugador tras detectarlo. El GameManager cambia a estado GameOver y reinicia la escena.

## Tabla de autoria

| Modulo | Responsable | Aportacion |
| --- | --- | --- |
| Gameplay / escena principal | Equipo | Integracion de escena jugable, game loop y condicion de victoria/derrota |
| IA y NavMesh | Equipo | NPCs, estados, raycast y navegacion |
| Audio | Equipo | Mixer, audio 3D, pasos por superficie y musica adaptativa |
| Persistencia | Equipo | JSON, multiples slots y UI de guardado/carga |
| UI y pulido | Equipo | HUD, pausa, guardado, feedback y salida de build |
| Documentacion | Equipo | GDD, arquitectura, persistencia y optimizacion |
