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
  <div class="min-h-screen bg-[#f7f5f0] p-6 font-sans">
    <div class="max-w-4xl mx-auto space-y-4">
      
      <div>
        <button 
          @click="router.push('/solicitudes')"
          class="inline-flex items-center gap-1.5 text-xs font-bold text-[#204060] hover:text-[#e48338] transition-colors bg-white px-3 py-1.5 rounded border border-gray-200 shadow-sm"><span>← Volver al listado</span>
        </button>
      </div>

      <!-- Estado: Cargando -->
      <div v-if="store.loading" class="bg-white border border-gray-200 rounded p-6 text-center text-[#204060] font-medium text-sm shadow-sm">
        Cargando información de la solicitud...
      </div>

      <!-- Estado: Error -->
      <div v-else-if="store.error" class="bg-red-50 border border-red-200 text-red-700 p-4 rounded text-sm font-medium shadow-sm">
        {{ store.error }}
      </div>

      <!-- Estado: Detalle de Solicitud -->
      <div v-else-if="store.solicitud" class="bg-white border border-gray-200 rounded shadow-sm overflow-hidden">
        
        <!-- Header de la Tarjeta -->
        <div class="p-6 border-b border-gray-200 flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <div class="flex items-center gap-3 mb-2">
              <span class="text-xs font-bold uppercase tracking-wider text-[#204060] bg-[#f7f5f0] px-2.5 py-1 rounded border border-gray-200" data-testid="detalle-codigo">
                Código: {{ store.solicitud.codigo }}
              </span>
              <span data-testid="detalle-estado" class="text-xs font-semibold px-2.5 py-1 rounded border border-gray-300 bg-gray-100 text-gray-700">
                {{ store.solicitud.estado }}
              </span>
              <span 
                v-if="store.solicitud.fechaLimiteSla && new Date(store.solicitud.fechaLimiteSla) < new Date() && !['Resuelta','Cerrada','Cancelada'].includes(store.solicitud.estado)" 
                data-testid="detalle-vencida" 
                class="text-xs font-bold px-2.5 py-1 rounded bg-red-100 border border-red-200 text-red-700"
              >
                VENCIDA
              </span>
            </div>
            <h1 class="text-2xl font-bold text-[#204060]" data-testid="detalle-titulo">
              {{ store.solicitud.titulo }}
            </h1>
          </div>

          <!-- Botón Editar -->
          <button 
            v-if="puedeEditar" 
            data-testid="btn-editar" 
            @click="router.push(`/solicitudes/${id}/editar`)"
            class="bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-medium text-sm py-2 px-4 rounded transition-colors"
          >
            Editar
          </button>
        </div>

        <!-- Cuerpo del Detalle -->
        <div class="p-6 space-y-6">
          
          <!-- Descripción -->
          <div>
            <h2 class="text-xs font-bold text-gray-500 uppercase tracking-wider mb-2">Descripción</h2>
            <div class="p-4 bg-[#f7f5f0] border border-gray-200 rounded text-sm text-gray-800 whitespace-pre-line" data-testid="detalle-descripcion">
              {{ store.solicitud.descripcion }}
            </div>
          </div>

          <!-- Información General (Grid Plano) -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            
            <div class="p-3 border border-gray-200 rounded bg-white">
              <span class="block text-xs font-semibold text-gray-500">Categoría</span>
              <span class="font-medium text-gray-800" data-testid="detalle-categoria">
                {{ store.solicitud.categoria.nombre }}
              </span>
            </div>

            <div class="p-3 border border-gray-200 rounded bg-white">
              <span class="block text-xs font-semibold text-gray-500">Prioridad</span>
              <span class="font-medium text-gray-800" data-testid="detalle-prioridad">
                {{ store.solicitud.prioridad }}
              </span>
            </div>

            <div class="p-3 border border-gray-200 rounded bg-white">
              <span class="block text-xs font-semibold text-gray-500">Agente</span>
              <span class="font-medium text-gray-800" data-testid="detalle-agente">
                {{ store.solicitud.agente?.nombre ?? 'Sin asignar' }}
              </span>
            </div>

            <div class="p-3 border border-gray-200 rounded bg-white">
              <span class="block text-xs font-semibold text-gray-500">Fecha de creación</span>
              <span class="font-medium text-gray-800" data-testid="detalle-fecha-creacion">
                {{ new Date(store.solicitud.fechaCreacion).toLocaleString() }}
              </span>
            </div>

            <div class="p-3 border border-gray-200 rounded bg-white">
              <span class="block text-xs font-semibold text-gray-500">Fecha límite SLA</span>
              <span class="font-medium text-gray-800" data-testid="detalle-fecha-limite">
                {{ new Date(store.solicitud.fechaLimiteSla).toLocaleString() }}
              </span>
            </div>

            <div v-if="store.solicitud.fechaResolucion" class="p-3 border border-gray-200 rounded bg-white">
              <span class="block text-xs font-semibold text-gray-500">Fecha de resolución</span>
              <span class="font-medium text-gray-800">
                {{ new Date(store.solicitud.fechaResolucion).toLocaleString() }}
              </span>
            </div>

          </div>

          <!-- Motivo de Resolución / Cancelación -->
          <div v-if="store.solicitud.motivoResolucion" class="p-4 bg-emerald-50 border border-emerald-200 rounded text-sm text-emerald-900" data-testid="detalle-motivo">
            <span class="block font-semibold mb-1">Motivo de resolución:</span>
            {{ store.solicitud.motivoResolucion }}
          </div>

          <div v-if="store.solicitud.motivoCancelacion" class="p-4 bg-red-50 border border-red-200 rounded text-sm text-red-900" data-testid="detalle-motivo">
            <span class="block font-semibold mb-1">Motivo de cancelación:</span>
            {{ store.solicitud.motivoCancelacion }}
          </div>

          <!-- Botones de Acción (Barra Inferior) -->
          <div v-if="accionesDisponibles.length > 0" class="pt-4 border-t border-gray-200">
            <h2 class="text-xs font-bold text-gray-500 uppercase tracking-wider mb-3">Acciones disponibles</h2>
            <div class="flex flex-wrap gap-2">
              
              <button 
                v-if="accionesDisponibles.includes('asignar')" 
                data-testid="btn-accion-asignar" 
                @click="abrirModal('asignar')"
                class="bg-[#204060] hover:bg-[#152a40] text-white font-medium text-sm py-2 px-4 rounded transition-colors"
              >
                Asignar
              </button>

              <button 
                v-if="accionesDisponibles.includes('iniciar')" 
                data-testid="btn-accion-iniciar" 
                @click="abrirModal('iniciar')"
                class="bg-[#e48338] hover:bg-[#c96f28] text-white font-medium text-sm py-2 px-4 rounded transition-colors"
              >
                Iniciar
              </button>

              <button 
                v-if="accionesDisponibles.includes('resolver')" 
                data-testid="btn-accion-resolver" 
                @click="abrirModal('resolver')"
                class="bg-emerald-700 hover:bg-emerald-800 text-white font-medium text-sm py-2 px-4 rounded transition-colors"
              >
                Resolver
              </button>

              <button 
                v-if="accionesDisponibles.includes('cerrar')" 
                data-testid="btn-accion-cerrar" 
                @click="abrirModal('cerrar')"
                class="bg-gray-700 hover:bg-gray-800 text-white font-medium text-sm py-2 px-4 rounded transition-colors"
              >
                Cerrar
              </button>

              <button 
                v-if="accionesDisponibles.includes('reabrir')" 
                data-testid="btn-accion-reabrir" 
                @click="abrirModal('reabrir')"
                class="bg-amber-600 hover:bg-amber-700 text-white font-medium text-sm py-2 px-4 rounded transition-colors"
              >
                Reabrir
              </button>

              <button 
                v-if="accionesDisponibles.includes('cancelar')" 
                data-testid="btn-accion-cancelar" 
                @click="abrirModal('cancelar')"
                class="bg-red-700 hover:bg-red-800 text-white font-medium text-sm py-2 px-4 rounded transition-colors"
              >
                Cancelar
              </button>

            </div>
          </div>

        </div>
      </div>

      <!-- Modal de Acción Plano -->
      <div 
        v-if="modalAccion" 
        data-testid="modal-accion"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50"
      >
        <div class="bg-white rounded border border-gray-300 shadow-md max-w-md w-full p-6">
          
          <h2 class="text-lg font-bold text-[#204060] mb-4 border-b border-gray-100 pb-2 capitalize">
            Confirmar {{ modalAccion }}
          </h2>

          <!-- Modal: Asignar -->
          <div v-if="modalAccion === 'asignar'" class="mb-4">
            <label class="block text-xs font-semibold text-gray-700 mb-1.5">Agente</label>
            <select 
              v-model="modalAgenteId" 
              data-testid="modal-select-agente"
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-[#204060] focus:border-[#204060] outline-none text-gray-800 bg-white"
            >
              <option value="">Seleccione un agente</option>
              <option v-for="agente in agentes" :key="agente.id" :value="agente.id">
                {{ agente.nombre }}
              </option>
            </select>
          </div>

          <!-- Modal: Resolver o Cancelar -->
          <div v-if="modalAccion === 'resolver' || modalAccion === 'cancelar'" class="mb-4">
            <label class="block text-xs font-semibold text-gray-700 mb-1.5">Motivo</label>
            <textarea 
              v-model="modalMotivo" 
              data-testid="modal-motivo" 
              placeholder="Ingrese el motivo..."
              rows="4"
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-[#204060] focus:border-[#204060] outline-none text-gray-800 bg-white resize-none"
            ></textarea>
          </div>

          <!-- Modal: Error -->
          <div 
            v-if="modalError" 
            data-testid="modal-error"
            class="text-xs font-medium text-red-700 bg-red-50 border border-red-200 p-2.5 rounded mb-4"
          >
            {{ modalError }}
          </div>

          <!-- Modal: Botones de Acción -->
          <div class="flex items-center justify-end gap-2 pt-2">
            <button 
              @click="modalAccion = null" 
              data-testid="modal-cancelar"
              class="px-4 py-2 border border-gray-300 text-gray-700 font-medium text-sm rounded bg-white hover:bg-gray-50 transition-colors"
            >
              Cancelar
            </button>
            <button 
              @click="confirmarAccion" 
              data-testid="modal-confirmar"
              class="px-4 py-2 bg-[#204060] hover:bg-[#152a40] text-white font-medium text-sm rounded transition-colors"
            >
              Confirmar
            </button>
          </div>

        </div>
      </div>

    </div>
  </div>
</template>