<script setup lang="ts">
import { ref } from 'vue';

const props = defineProps<{
  accion: string;
  agentes: { id: string; nombre: string }[];
}>();

const emit = defineEmits<{
  confirmar: [payload: { agenteId?: string; motivo?: string }];
  cancelar: [];
}>();

const modalAgenteId = ref('');
const modalMotivo = ref('');
const modalError = ref('');

function confirmar() {
  modalError.value = '';
  if (props.accion === 'asignar') {
    if (!modalAgenteId.value) {
      modalError.value = 'Debe seleccionar un agente';
      return;
    }
    emit('confirmar', { agenteId: modalAgenteId.value });
  } else if (props.accion === 'resolver' || props.accion === 'cancelar') {
    const minLength = props.accion === 'resolver' ? 20 : 10;
    if (!modalMotivo.value || modalMotivo.value.length < minLength) {
      modalError.value = `El motivo debe tener al menos ${minLength} caracteres`;
      return;
    }
    emit('confirmar', { motivo: modalMotivo.value });
  } else {
    emit('confirmar', {});
  }
}

function cancelar() {
  emit('cancelar');
}
</script>

<template>
  <div data-testid="modal-accion" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/50">
    <div class="bg-white rounded border border-gray-300 shadow-md max-w-md w-full p-6">
      <h2 class="text-lg font-bold text-[#204060] mb-4 border-b border-gray-100 pb-2 capitalize">
        Confirmar {{ accion }}
      </h2>

      <!-- Asignar -->
      <div v-if="accion === 'asignar'" class="mb-4">
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

      <!-- Resolver / Cancelar -->
      <div v-if="accion === 'resolver' || accion === 'cancelar'" class="mb-4">
        <label class="block text-xs font-semibold text-gray-700 mb-1.5">Motivo</label>
        <textarea
          v-model="modalMotivo"
          data-testid="modal-motivo"
          placeholder="Ingrese el motivo..."
          rows="4"
          class="w-full px-3 py-2 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-[#204060] focus:border-[#204060] outline-none text-gray-800 bg-white resize-none"
        ></textarea>
      </div>

      <!-- Error -->
      <div
        v-if="modalError"
        data-testid="modal-error"
        class="text-xs font-medium text-red-700 bg-red-50 border border-red-200 p-2.5 rounded mb-4"
      >
        {{ modalError }}
      </div>

      <!-- Botones -->
      <div class="flex items-center justify-end gap-2 pt-2">
        <button
          @click="cancelar"
          data-testid="modal-cancelar"
          class="px-4 py-2 border border-gray-300 text-gray-700 font-medium text-sm rounded bg-white hover:bg-gray-50 transition-colors"
        >
          Cancelar
        </button>
        <button
          @click="confirmar"
          data-testid="modal-confirmar"
          class="px-4 py-2 bg-[#204060] hover:bg-[#152a40] text-white font-medium text-sm rounded transition-colors"
        >
          Confirmar
        </button>
      </div>
    </div>
  </div>
</template>