<template>
  <div>
    <!-- Selector -->
    <div class="card">
      <div class="section-header">
        <h2 class="card-title" style="margin:0">Przeglądy techniczne</h2>
      </div>
      <div class="form-row" style="margin-bottom:0">
        <div class="form-group" style="max-width:340px">
          <label>Wybierz pojazd</label>
          <select v-model="selectedId" @change="onVehicleChange">
            <option value="">— wybierz pojazd —</option>
            <option v-for="v in vehicles" :key="v.id" :value="v.id">
              {{ v.brand }} {{ v.model }} · {{ v.licensePlate }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <template v-if="selectedId">
      <!-- Maintenance status -->
      <div class="card" v-if="maintStatus && maintStatus.length">
        <h3 class="card-title">Status przeglądów</h3>
        <ul class="status-list">
          <li v-for="ms in maintStatus" :key="ms.maintenanceTypeId" :class="['status-item', slClass(ms.statusLevel)]">
            <div style="display:flex;align-items:center;gap:8px;flex-wrap:wrap">
              <span class="status-item-name">{{ ms.maintenanceTypeName }}</span>
              <span :class="['badge', slBadge(ms.statusLevel)]">{{ ms.statusLevel }}</span>
            </div>
            <div class="status-item-msg">{{ ms.message }}</div>
            <div class="status-item-meta">
              <span v-if="ms.kilometersRemaining != null">Pozostało: <strong>{{ ms.kilometersRemaining?.toLocaleString('pl-PL') }} km</strong></span>
              <span v-if="ms.daysRemaining != null" style="margin-left:14px">/ <strong>{{ ms.daysRemaining }} dni</strong></span>
              <span v-if="ms.predictedMaintenanceDate" style="margin-left:14px">Termin: <strong>{{ fmtDate(ms.predictedMaintenanceDate) }}</strong></span>
            </div>
          </li>
        </ul>
      </div>

      <!-- Add maintenance event -->
      <div class="card">
        <h3 class="card-title">Dodaj przegląd / naprawę</h3>
        <div class="form-row">
          <div class="form-group">
            <label>Typ przeglądu *</label>
            <select v-model.number="addForm.maintenanceTypeId">
              <option value="">— wybierz —</option>
              <option v-for="t in types" :key="t.id" :value="t.id">{{ t.name }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Data i godzina *</label>
            <input v-model="addForm.date" type="datetime-local" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Stan licznika (km) *</label>
            <input v-model.number="addForm.odometerReading" type="number" min="0" max="2000000" placeholder="np. 80000" />
          </div>
          <div class="form-group">
            <label>Koszt całkowity (zł) *</label>
            <input v-model.number="addForm.totalCost" type="number" step="0.01" min="0.01" max="100000" placeholder="np. 350.00" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Opis * (max 500 znaków)</label>
            <textarea v-model="addForm.description" rows="3" maxlength="500"
              placeholder="np. Wymiana oleju silnikowego i filtra oleju, wymiana filtra powietrza..."></textarea>
            <small style="color:#9ca3af;font-size:0.72rem">{{ addForm.description?.length ?? 0 }} / 500</small>
          </div>
        </div>
        <div v-if="addError" class="error-msg">{{ addError }}</div>
        <button class="btn btn-primary" @click="submitAdd" :disabled="saving">
          {{ saving ? 'Dodawanie...' : '+ Dodaj przegląd' }}
        </button>
      </div>

      <!-- History -->
      <div class="card">
        <h3 class="card-title">Historia przeglądów</h3>
        <div v-if="loadingHistory" class="loading">Ładowanie historii...</div>
        <table v-else>
          <thead>
            <tr>
              <th>ID</th>
              <th>Data</th>
              <th>Typ</th>
              <th>Licznik (km)</th>
              <th>Koszt (zł)</th>
              <th>Opis</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="!history.length">
              <td colspan="7" class="empty">Brak zdarzeń przeglądów</td>
            </tr>
            <tr v-for="e in history" :key="e.id">
              <td style="color:#9ca3af;font-size:0.8rem">#{{ e.id }}</td>
              <td>{{ fmtDate(e.date) }}</td>
              <td>
                <span class="badge badge-blue">{{ e.maintenanceTypeName }}</span>
              </td>
              <td>{{ e.odometerReading.toLocaleString('pl-PL') }}</td>
              <td>{{ e.totalCost.toFixed(2) }} zł</td>
              <td style="max-width:220px">
                <span :title="e.description" style="display:block;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                  {{ e.description }}
                </span>
              </td>
              <td>
                <button class="btn btn-danger btn-sm" @click="doDelete(e)">Usuń</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <div v-else class="card" style="text-align:center;color:#9ca3af;padding:48px">
      Wybierz pojazd, aby zobaczyć historię przeglądów
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { vehiclesApi, maintenanceApi, maintenanceTypesApi } from '../api.js'

const vehicles       = ref([])
const types          = ref([])
const selectedId     = ref('')
const history        = ref([])
const maintStatus    = ref(null)
const loadingHistory = ref(false)
const saving         = ref(false)
const addError       = ref('')

function freshForm() {
  return {
    maintenanceTypeId: '',
    date: new Date().toISOString().slice(0, 16),
    odometerReading: 0,
    totalCost: null,
    description: '',
  }
}
const addForm = ref(freshForm())

async function onVehicleChange() {
  if (!selectedId.value) return
  loadingHistory.value = true; maintStatus.value = null
  try {
    ;[history.value, maintStatus.value] = await Promise.all([
      maintenanceApi.getByVehicle(selectedId.value),
      vehiclesApi.getMaintenanceStatus(selectedId.value),
    ])
  } catch (e) { console.error(e) }
  finally { loadingHistory.value = false }
}

async function submitAdd() {
  saving.value = true; addError.value = ''
  try {
    await maintenanceApi.create({
      vehicleId: Number(selectedId.value),
      maintenanceTypeId: addForm.value.maintenanceTypeId,
      odometerReading: addForm.value.odometerReading,
      totalCost: addForm.value.totalCost,
      description: addForm.value.description,
      date: new Date(addForm.value.date).toISOString(),
    })
    addForm.value = freshForm()
    await onVehicleChange()
  } catch (e) { addError.value = e.message }
  finally { saving.value = false }
}

async function doDelete(e) {
  if (!confirm('Usunąć ten przegląd?')) return
  try { await maintenanceApi.delete(e.id); await onVehicleChange() }
  catch (err) { alert(err.message) }
}

function fmtDate(d) { return new Date(d).toLocaleDateString('pl-PL') }

function slClass(l = '') {
  const v = l.toLowerCase()
  if (v.includes('ok') || v.includes('good')) return 'ok'
  if (v.includes('overdue') || v.includes('critical')) return 'overdue'
  return 'warn'
}
function slBadge(l = '') {
  const v = l.toLowerCase()
  if (v.includes('ok') || v.includes('good')) return 'badge badge-green'
  if (v.includes('overdue') || v.includes('critical')) return 'badge badge-red'
  return 'badge badge-orange'
}

onMounted(async () => {
  try {
    ;[vehicles.value, types.value] = await Promise.all([
      vehiclesApi.getAll(),
      maintenanceTypesApi.getAll(),
    ])
  } catch {}
})
</script>
