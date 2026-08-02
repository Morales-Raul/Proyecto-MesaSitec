<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useSolicitudDetalleStore } from '../stores/solicitudDetalle';
import { useAuthStore } from '../stores/auth';
import type { TransicionRequest } from '../types';
import http from '../api/http';

const route = useRoute();
const router = useRouter();
const store = useSolicitudDetalleStore();
const auth = useAuthStore();

const id = route.params.id as string;

// Modal genérico
const modalAccion = ref<string | null>(null);
const modalAgenteId = ref('');
const modalMotivo = ref('');
const modalError = ref('');
const agentes = ref<{ id: string; nombre: string }[]>([]);

onMounted(async () => {
  await store.cargar(id);
});

// Determinar qué acciones están disponibles según RN-02 y RN-03
const accionesDisponibles = computed(() => {
  const estado = store.solicitud?.estado;
  const rol = auth.usuario?.rol;
  const esPropia = store.solicitud?.solicitante.id === auth.usuario?.id;

  const disponibles: string[] = [];
  if (!estado || !rol) return disponibles;

  // Admin puede todo excepto cancelar (que ya está en la tabla)
  // Agente no puede cancelar
  // Solicitante solo puede cerrar su propia solicitud

  const transiciones: Record<string, string[]> = {
    Nueva: ['asignar', 'cancelar'],
    Asignada: ['iniciar', 'asignar', 'cancelar'],
    EnProceso: ['resolver', 'asignar', 'cancelar'],
    Resuelta: ['cerrar', 'reabrir'],
  };

  const posibles = transiciones[estado] || [];

  for (const accion of posibles) {
    if (rol === 'Admin') {
      disponibles.push(accion);
    } else if (rol === 'Agente') {
      if (accion !== 'cancelar') disponibles.push(accion);
    } else if (rol === 'Solicitante') {
      if (accion === 'cerrar' && esPropia) disponibles.push(accion);
    }
  }

  return disponibles;
});

function abrirModal(accion: string) {
  modalAccion.value = accion;
  modalError.value = '';
  modalMotivo.value = '';
  if (accion === 'asignar') {
    cargarAgentes();
  }
}

async function cargarAgentes() {
   try {
    const { data } = await http.get<{ id: string; nombre: string }[]>('/usuarios/agentes');
    agentes.value = data;
  } catch {

  }
}

async function confirmarAccion() {
  if (!modalAccion.value) return;
  modalError.value = '';

  const request: TransicionRequest = { accion: modalAccion.value };
  if (modalAccion.value === 'asignar') {
    if (!modalAgenteId.value) {
      modalError.value = 'Debe seleccionar un agente';
      return;
    }
    request.agenteId = modalAgenteId.value;
  } else if (modalAccion.value === 'resolver' || modalAccion.value === 'cancelar') {
    if (!modalMotivo.value || modalMotivo.value.length < (modalAccion.value === 'resolver' ? 20 : 10)) {
      modalError.value = `El motivo debe tener al menos ${modalAccion.value === 'resolver' ? 20 : 10} caracteres`;
      return;
    }
    request.motivo = modalMotivo.value;
  }

  const ok = await store.ejecutarTransicion(id, request);
  if (ok) {
    modalAccion.value = null;
  } else {
    modalError.value = store.error || 'Error al ejecutar la acción';
  }
}

const puedeEditar = computed(() => {
  const rol = auth.usuario?.rol;
  const estado = store.solicitud?.estado;
  const esPropia = store.solicitud?.solicitante.id === auth.usuario?.id;
  if (!rol || !estado) return false;
  if (rol === 'Admin' || rol === 'Agente') return true;
  if (rol === 'Solicitante') return esPropia && estado === 'Nueva';
  return false;
});

</script>

<template>
  <div v-if="store.loading">Cargando...</div>
  <div v-else-if="store.error">{{ store.error }}</div>
  <div v-else-if="store.solicitud">
    <h1>Detalle de Solicitud</h1>

    <p data-testid="detalle-codigo"><strong>Código:</strong> {{ store.solicitud.codigo }}</p>
    <p data-testid="detalle-titulo"><strong>Título:</strong> {{ store.solicitud.titulo }}</p>
    <p data-testid="detalle-descripcion"><strong>Descripción:</strong> {{ store.solicitud.descripcion }}</p>
    <p data-testid="detalle-estado"><strong>Estado:</strong> {{ store.solicitud.estado }}</p>
    <p data-testid="detalle-prioridad"><strong>Prioridad:</strong> {{ store.solicitud.prioridad }}</p>
    <p data-testid="detalle-categoria"><strong>Categoría:</strong> {{ store.solicitud.categoria.nombre }}</p>
    <p data-testid="detalle-agente"><strong>Agente:</strong> {{ store.solicitud.agente?.nombre ?? 'Sin asignar' }}</p>
    <p data-testid="detalle-fecha-creacion"><strong>Fecha creación:</strong> {{ new Date(store.solicitud.fechaCreacion).toLocaleString() }}</p>
    <p data-testid="detalle-fecha-limite"><strong>Fecha límite SLA:</strong> {{ new Date(store.solicitud.fechaLimiteSla).toLocaleString() }}</p>
    <p v-if="store.solicitud.fechaResolucion"><strong>Fecha resolución:</strong> {{ new Date(store.solicitud.fechaResolucion).toLocaleString() }}</p>
    <p v-if="store.solicitud.motivoResolucion" data-testid="detalle-motivo"><strong>Motivo resolución:</strong> {{ store.solicitud.motivoResolucion }}</p>
    <p v-if="store.solicitud.motivoCancelacion" data-testid="detalle-motivo"><strong>Motivo cancelación:</strong> {{ store.solicitud.motivoCancelacion }}</p>
    <p v-if="store.solicitud.fechaLimiteSla && new Date(store.solicitud.fechaLimiteSla) < new Date() && !['Resuelta','Cerrada','Cancelada'].includes(store.solicitud.estado)" data-testid="detalle-vencida" style="color:red">VENCIDA</p>

    <!-- Botones dinámicos -->
    <div>
      <button v-if="accionesDisponibles.includes('asignar')" data-testid="btn-accion-asignar" @click="abrirModal('asignar')">Asignar</button>
      <button v-if="accionesDisponibles.includes('iniciar')" data-testid="btn-accion-iniciar" @click="abrirModal('iniciar')">Iniciar</button>
      <button v-if="accionesDisponibles.includes('resolver')" data-testid="btn-accion-resolver" @click="abrirModal('resolver')">Resolver</button>
      <button v-if="accionesDisponibles.includes('cerrar')" data-testid="btn-accion-cerrar" @click="abrirModal('cerrar')">Cerrar</button>
      <button v-if="accionesDisponibles.includes('reabrir')" data-testid="btn-accion-reabrir" @click="abrirModal('reabrir')">Reabrir</button>
      <button v-if="accionesDisponibles.includes('cancelar')" data-testid="btn-accion-cancelar" @click="abrirModal('cancelar')">Cancelar</button>
      <button v-if="puedeEditar" data-testid="btn-editar" @click="router.push(`/solicitudes/${id}/editar`)">Editar</button>
    </div>

    <!-- Modal de acción -->
    <div v-if="modalAccion" data-testid="modal-accion">
      <h2>Confirmar {{ modalAccion }}</h2>

      <div v-if="modalAccion === 'asignar'">
        <select v-model="modalAgenteId" data-testid="modal-select-agente">
          <option value="">Seleccione un agente</option>
          <option v-for="agente in agentes" :key="agente.id" :value="agente.id">{{ agente.nombre }}</option>
        </select>
      </div>

      <div v-if="modalAccion === 'resolver' || modalAccion === 'cancelar'">
        <textarea v-model="modalMotivo" data-testid="modal-motivo" placeholder="Ingrese el motivo..."></textarea>
      </div>

      <p v-if="modalError" data-testid="modal-error">{{ modalError }}</p>

      <button @click="confirmarAccion" data-testid="modal-confirmar">Confirmar</button>
      <button @click="modalAccion = null" data-testid="modal-cancelar">Cancelar</button>
    </div>
  </div>
</template>