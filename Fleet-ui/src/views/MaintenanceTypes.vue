<template>
  <div class="card">
    <h2 class="card-title">Typy przeglądów</h2>
    <p style="font-size:0.85rem;color:#6b7280;margin-bottom:18px;margin-top:-10px">
      Słownik typów przeglądów zdefiniowanych w systemie. Interwały domyślne są używane przez moduł analizy przeglądów.
    </p>

    <div v-if="loading" class="loading">Ładowanie...</div>
    <div v-else-if="error" class="error-msg">{{ error }}</div>
    <table v-else>
      <thead>
        <tr>
          <th>ID</th>
          <th>Nazwa</th>
          <th>Kod systemowy</th>
          <th>Interwał (km)</th>
          <th>Interwał (dni)</th>
          <th>Typ interwału</th>
        </tr>
      </thead>
      <tbody>
        <tr v-if="!types.length">
          <td colspan="6" class="empty">Brak typów przeglądów</td>
        </tr>
        <tr v-for="t in types" :key="t.id">
          <td style="color:#9ca3af;font-size:0.8rem">#{{ t.id }}</td>
          <td><strong>{{ t.name }}</strong></td>
          <td><code style="background:#f3f4f6;padding:2px 7px;border-radius:4px;font-size:0.82rem">{{ t.systemCode }}</code></td>
          <td>
            <span v-if="t.defaultIntervalOdometer" class="badge badge-blue">
              {{ t.defaultIntervalOdometer.toLocaleString('pl-PL') }} km
            </span>
            <span v-else style="color:#d1d5db">—</span>
          </td>
          <td>
            <span v-if="t.defaultIntervalDays" class="badge badge-orange">
              {{ t.defaultIntervalDays }} dni
            </span>
            <span v-else style="color:#d1d5db">—</span>
          </td>
          <td>
            <span v-if="t.defaultIntervalOdometer && t.defaultIntervalDays" class="badge badge-gray">Dystans + czas</span>
            <span v-else-if="t.defaultIntervalOdometer" class="badge badge-blue">Dystans</span>
            <span v-else-if="t.defaultIntervalDays" class="badge badge-orange">Czas</span>
            <span v-else style="color:#d1d5db">—</span>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="types.length" style="margin-top:16px;padding-top:14px;border-top:1px solid #f1f4f9;font-size:0.8rem;color:#9ca3af">
      Łącznie {{ types.length }} typ{{ types.length === 1 ? '' : 'ów' }} przeglądów
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { maintenanceTypesApi } from '../api.js'

const types   = ref([])
const loading = ref(false)
const error   = ref('')

onMounted(async () => {
  loading.value = true
  try { types.value = await maintenanceTypesApi.getAll() }
  catch (e) { error.value = e.message }
  finally { loading.value = false }
})
</script>
