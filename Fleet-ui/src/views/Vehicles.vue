<template>
  <div>
    <!-- Vehicles list -->
    <div class="card">
      <div class="section-header">
        <h2 class="card-title" style="margin:0">Pojazdy</h2>
        <button class="btn btn-primary" @click="openAdd">+ Dodaj pojazd</button>
      </div>

      <div v-if="loading" class="loading">Ładowanie pojazdów...</div>
      <div v-else-if="error" class="error-msg">{{ error }}</div>

      <table v-else>
        <thead>
          <tr>
            <th>ID</th>
            <th>Marka / Model</th>
            <th>Rejestracja</th>
            <th>VIN</th>
            <th>Rok</th>
            <th>Licznik (km)</th>
            <th>Status</th>
            <th>Akcje</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!vehicles.length">
            <td colspan="8" class="empty">Brak pojazdów w bazie</td>
          </tr>
          <tr v-for="v in vehicles" :key="v.id">
            <td style="color:#9ca3af;font-size:0.8rem">#{{ v.id }}</td>
            <td><strong>{{ v.brand }} {{ v.model }}</strong></td>
            <td><span style="font-family:monospace;font-weight:700">{{ v.licensePlate }}</span></td>
            <td style="font-family:monospace;font-size:0.78rem;color:#6b7280">{{ v.vin }}</td>
            <td>{{ v.year }}</td>
            <td>{{ v.odometerReading.toLocaleString('pl-PL') }}</td>
            <td><span :class="statusBadge(v.status)" class="badge">{{ statusLabel(v.status) }}</span></td>
            <td>
              <div class="actions">
                <button class="btn btn-secondary btn-sm" @click="openDetails(v)">Szczegóły</button>
                <button class="btn btn-primary btn-sm" @click="openEdit(v)">Edytuj</button>
                <button class="btn btn-warning btn-sm" @click="openCalibrate(v)">Licznik</button>
                <button v-if="v.status === 'Idle'" class="btn btn-success btn-sm" @click="doStartTrip(v)">▶ Start</button>
                <button v-if="v.status === 'InTransit'" class="btn btn-warning btn-sm" @click="doEndTrip(v)">■ Stop</button>
                <button class="btn btn-danger btn-sm" @click="doDelete(v)">Usuń</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ── Add / Edit modal ── -->
    <div v-if="showForm" class="modal-overlay" @click.self="showForm = false">
        <div class="modal">
            <div class="modal-title">{{ editing ? 'Edytuj pojazd' : 'Dodaj nowy pojazd' }}</div>

            <div class="form-row">
                <div class="form-group">
                    <label>Marka *</label>
                    <input v-model="form.brand" placeholder="np. Toyota" />
                </div>
                <div class="form-group">
                    <label>Model *</label>
                    <input v-model="form.model" placeholder="np. Corolla" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label>Rejestracja * (max 10 zn.)</label>
                    <input v-model="form.licensePlate" maxlength="10" placeholder="np. KI 12345" style="text-transform:uppercase" />
                </div>
                <div class="form-group" v-if="editing">
                    <label>Rok produkcji *</label>
                    <input v-model.number="form.year" type="number" min="1900" max="2030" placeholder="2020" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label>Pojemność baku (L) *</label>
                    <input v-model.number="form.fuelTankCapacity" type="number" min="1" step="1" placeholder="np. 50" />
                </div>
                <div class="form-group">
                    <label>Spalanie fabryczne (L/100km) *</label>
                    <input v-model.number="form.fuelConsumption" type="number" min="0.1" step="0.1" placeholder="np. 7.5" />
                </div>
            </div>

            <template v-if="!editing">
                <div class="form-row">
                    <div class="form-group">
                        <label>VIN * (dokładnie 17 znaków)</label>
                        <input v-model="form.vin" maxlength="17" placeholder="1HGBH41JXMN109186" style="font-family:monospace;text-transform:uppercase" />
                        <small style="color:#9ca3af;font-size:0.75rem">Wpisano: {{ form.vin?.length ?? 0 }} / 17</small>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-group">
                        <label>Stan licznika (km)</label>
                        <input v-model.number="form.odometerReading" type="number" min="0" placeholder="0" />
                    </div>
                </div>
            </template>

            <div v-if="formError" class="error-msg">{{ formError }}</div>
            <div class="modal-actions">
                <button class="btn btn-secondary" @click="showForm = false">Anuluj</button>
                <button class="btn btn-primary" @click="submitForm" :disabled="saving">{{ saving ? 'Zapisywanie...' : 'Zapisz' }}</button>
            </div>
        </div>
    </div>

    <!-- ── Calibrate odometer modal ── -->
    <div v-if="showCalibrate" class="modal-overlay" @click.self="showCalibrate = false">
      <div class="modal">
        <div class="modal-title">Kalibracja licznika — {{ calTarget?.brand }} {{ calTarget?.model }}</div>
        <div class="form-row">
          <div class="form-group">
            <label>Aktualny stan</label>
            <input :value="calTarget?.odometerReading.toLocaleString('pl-PL') + ' km'" disabled style="background:#f9fafb" />
          </div>
          <div class="form-group">
            <label>Nowy stan licznika (km) *</label>
            <input v-model.number="calForm.newOdometerReading" type="number" min="0" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Uzasadnienie * (min. 5 znaków)</label>
            <textarea v-model="calForm.justification" rows="3" placeholder="np. Wymiana licznika po naprawie..."></textarea>
          </div>
        </div>
        <div v-if="calError" class="error-msg">{{ calError }}</div>
        <div class="modal-actions">
          <button class="btn btn-secondary" @click="showCalibrate = false">Anuluj</button>
          <button class="btn btn-warning" @click="submitCalibrate" :disabled="saving">{{ saving ? '...' : 'Kalibruj' }}</button>
        </div>
      </div>
    </div>

    <!-- ── Details modal ── -->
    <div v-if="showDetails" class="modal-overlay" @click.self="showDetails = false">
      <div class="modal" style="width:680px">
        <div class="modal-title">
          {{ det?.brand }} {{ det?.model }}
          <span :class="statusBadge(det?.status)" class="badge" style="margin-left:10px;font-size:0.8rem">{{ statusLabel(det?.status) }}</span>
        </div>

        <div style="display:grid;grid-template-columns:1fr 1fr;gap:8px;margin-bottom:20px;font-size:0.85rem">
            <div><span style="color:#9ca3af">VIN:</span> <code>{{ det?.vin }}</code></div>
            <div><span style="color:#9ca3af">Rejestracja:</span> <strong>{{ det?.licensePlate }}</strong></div>
            <div><span style="color:#9ca3af">Rok:</span> {{ det?.year }}</div>
            <div><span style="color:#9ca3af">Licznik:</span> <strong>{{ det?.odometerReading?.toLocaleString('pl-PL') }} km</strong></div>
            <div><span style="color:#9ca3af">Bak:</span> {{ det?.fuelTankCapacity }} L</div>
            <div><span style="color:#9ca3af">Spalanie normatywne:</span> {{ det?.fuelConsumption }} L/100km</div>
        </div>

        <!-- Fuel stats -->
        <div style="font-size:0.82rem;font-weight:700;text-transform:uppercase;letter-spacing:0.05em;color:#6b7280;margin-bottom:10px">Statystyki paliwa</div>
        <div v-if="loadingStats" class="loading" style="padding:16px">Ładowanie...</div>
        <div v-else-if="fuelStats" class="stat-grid" style="margin-bottom:20px">
          <div class="stat-card">
            <div class="stat-value">{{ fuelStats.totalDistanceKm?.toLocaleString('pl-PL') }}</div>
            <div class="stat-label">Km łącznie</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ fuelStats.totalFuelLiters?.toFixed(1) }} L</div>
            <div class="stat-label">Paliwo łącznie</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ fuelStats.totalCost?.toFixed(2) }} zł</div>
            <div class="stat-label">Koszt łączny</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ fuelStats.averageConsumption?.toFixed(1) }}</div>
            <div class="stat-label">L / 100 km</div>
          </div>
        </div>
        <div v-else style="color:#9ca3af;font-size:0.82rem;margin-bottom:16px">Brak danych o tankowaniach</div>

        <!-- Maintenance status -->
        <div style="font-size:0.82rem;font-weight:700;text-transform:uppercase;letter-spacing:0.05em;color:#6b7280;margin-bottom:10px">Status przeglądów</div>
        <div v-if="loadingMaint" class="loading" style="padding:16px">Ładowanie...</div>
        <ul v-else-if="maintStatus && maintStatus.length" class="status-list">
          <li v-for="ms in maintStatus" :key="ms.maintenanceTypeId" :class="['status-item', slClass(ms.statusLevel)]">
            <div style="display:flex;align-items:center;gap:8px">
              <span class="status-item-name">{{ ms.maintenanceTypeName }}</span>
              <span :class="['badge', slBadge(ms.statusLevel)]">{{ ms.statusLevel }}</span>
            </div>
            <div class="status-item-msg">{{ ms.message }}</div>
            <div class="status-item-meta">
              <span v-if="ms.kilometersRemaining != null">Pozostało: {{ ms.kilometersRemaining?.toLocaleString('pl-PL') }} km</span>
              <span v-if="ms.daysRemaining != null" style="margin-left:12px">/ {{ ms.daysRemaining }} dni</span>
              <span v-if="ms.predictedMaintenanceDate" style="margin-left:12px">Termin: {{ fmtDate(ms.predictedMaintenanceDate) }}</span>
            </div>
          </li>
        </ul>
        <div v-else style="color:#9ca3af;font-size:0.82rem">Brak danych o przeglądach</div>

        <div class="modal-actions">
          <button class="btn btn-secondary" @click="showDetails = false">Zamknij</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { vehiclesApi } from '../api.js'

const vehicles = ref([])
const loading  = ref(false)
const error    = ref('')

// form
const showForm  = ref(false)
const editing   = ref(null)
const form      = ref({})
const formError = ref('')
const saving    = ref(false)

// calibrate
const showCalibrate = ref(false)
const calTarget = ref(null)
const calForm   = ref({ newOdometerReading: 0, justification: '' })
const calError  = ref('')

// details
const showDetails  = ref(false)
const det          = ref(null)
const fuelStats    = ref(null)
const maintStatus  = ref(null)
const loadingStats = ref(false)
const loadingMaint = ref(false)

async function load() {
  loading.value = true; error.value = ''
  try { vehicles.value = await vehiclesApi.getAll() }
  catch (e) { error.value = e.message }
  finally { loading.value = false }
}

/* ── helpers ── */
function statusLabel(s) { return { Idle: 'Wolny', InTransit: 'W trasie', Maintenance: 'Serwis' }[s] ?? s }
function statusBadge(s) { return { Idle: 'badge-green', InTransit: 'badge-blue', Maintenance: 'badge-orange' }[s] ?? 'badge-gray' }

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
function fmtDate(d) { return new Date(d).toLocaleDateString('pl-PL') }

/* ── Add / Edit ── */
function openAdd() {
  editing.value = null
    form.value = { brand: '', model: '', licensePlate: '', vin: '', odometerReading: 0, fuelTankCapacity: 50, fuelConsumption: 7.5  }
  formError.value = ''; showForm.value = true
}
function openEdit(v) {
  editing.value = v
    form.value = {
        brand: v.brand, model: v.model, licensePlate: v.licensePlate, year: v.year, rowVersion: v.rowVersion, fuelTankCapacity: v.fuelTankCapacity || 50,
        fuelConsumption: v.fuelConsumption || 7.5 }
  formError.value = ''; showForm.value = true
}
async function submitForm() {
  saving.value = true; formError.value = ''
  try {
    if (editing.value) {
      await vehiclesApi.update(editing.value.id, {
        licensePlate: form.value.licensePlate,
        brand: form.value.brand,
        model: form.value.model,
        year: form.value.year,
        rowVersion: form.value.rowVersion,
        fuelTankCapacity: form.value.fuelTankCapacity,
        fuelConsumption: form.value.fuelConsumption
      })
    } else {
      await vehiclesApi.create({
        vin: form.value.vin,
        licensePlate: form.value.licensePlate,
        brand: form.value.brand,
        model: form.value.model,
        odometerReading: form.value.odometerReading,
        fuelTankCapacity: form.value.fuelTankCapacity,
        fuelConsumption: form.value.fuelConsumption
      })
    }
    showForm.value = false
    await load()
  } catch (e) { formError.value = e.message }
  finally { saving.value = false }
}

/* ── Calibrate ── */
function openCalibrate(v) {
  calTarget.value = v
  calForm.value = { newOdometerReading: v.odometerReading, justification: '' }
  calError.value = ''; showCalibrate.value = true
}
async function submitCalibrate() {
  saving.value = true; calError.value = ''
  try {
    await vehiclesApi.calibrateOdometer(calTarget.value.id, calForm.value)
    showCalibrate.value = false; await load()
  } catch (e) { calError.value = e.message }
  finally { saving.value = false }
}

/* ── Delete ── */
async function doDelete(v) {
  if (!confirm(`Usunąć pojazd ${v.brand} ${v.model} (${v.licensePlate})?`)) return
  try { await vehiclesApi.delete(v.id); await load() }
  catch (e) { alert(e.message) }
}

/* ── Trip ── */
async function doStartTrip(v) {
  try { await vehiclesApi.startTrip(v.id); await load() }
  catch (e) { alert(e.message) }
}
async function doEndTrip(v) {
  try { await vehiclesApi.endTrip(v.id); await load() }
  catch (e) { alert(e.message) }
}

/* ── Details ── */
async function openDetails(v) {
  det.value = v; fuelStats.value = null; maintStatus.value = null
  showDetails.value = true

  loadingStats.value = true
  vehiclesApi.getFuelStats(v.id)
    .then(s => { fuelStats.value = s })
    .catch(() => {})
    .finally(() => { loadingStats.value = false })

  loadingMaint.value = true
  vehiclesApi.getMaintenanceStatus(v.id)
    .then(s => { maintStatus.value = s })
    .catch(() => {})
    .finally(() => { loadingMaint.value = false })
}

onMounted(load)
</script>
