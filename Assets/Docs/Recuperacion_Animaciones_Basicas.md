# Recuperacion - Animaciones basicas

## Objetivo

Se ha dejado un modulo de animacion sencillo y facil de defender: personaje humanoide, Animator Controller con Mecanim, Blend Tree para locomocion e IK basico de pies.

## Estructura

- `StarterAssetsThirdPerson.controller`: controlador Mecanim del personaje.
- Estado `Idle Walk Run Blend`: usa un Blend Tree 1D con el parametro `Speed`.
- Estados `JumpStart`, `InAir` y `JumpLand`: accion simple de salto integrada en el mismo Animator.
- `PlayerController`: actualiza los parametros `Speed`, `MotionSpeed`, `Grounded`, `FreeFall` y `Jump`.
- `SimpleFootIK`: ajusta los pies al suelo con raycasts.
- `RuntimeBootstrapper`: al cargar la escena, añade `SimpleFootIK` automaticamente a los animators humanoides que usan controller.

## Como funciona

El movimiento no cambia de golpe entre idle, caminar y correr. El script del jugador calcula la velocidad y la manda al Animator con el parametro `Speed`. El Blend Tree mezcla las animaciones segun ese valor:

- `0`: idle.
- `2`: caminar.
- `6`: correr.

Para el IK, `SimpleFootIK` lanza un raycast desde cada pie hacia abajo. Si encuentra suelo, coloca el pie cerca del punto de impacto y rota el pie siguiendo la normal del terreno. Es una version simple, suficiente para demostrar contacto con el suelo sin montar un sistema complejo.

Como accion adicional, el salto usa los parametros `Jump`, `Grounded` y `FreeFall` para pasar por inicio de salto, caida y aterrizaje.

## Verificacion

1. Entrar en Play Mode.
2. Mover el personaje despacio y despues correr.
3. Comprobar que la transicion idle/caminar/correr es fluida.
4. Pasar por una zona con desnivel o suelo irregular.
5. Comprobar que los pies intentan apoyarse sobre el suelo.

## Defensa tecnica corta

La solucion usa Mecanim porque ya estaba preparado en Unity y permite mezclar clips sin escribir mucha logica. El Blend Tree resuelve la locomocion con un unico parametro (`Speed`). El IK se ha hecho con raycasts simples para ajustar los pies al terreno, priorizando una implementacion clara y mantenible.
