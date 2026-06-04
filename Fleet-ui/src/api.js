const BASE = '/api'

async function req(url, options = {}) {
  const opts = { headers: { 'Content-Type': 'application/json' }, ...options }
  if (options.body !== undefined) opts.body = JSON.stringify(options.body)

  const res = await fetch(BASE + url, opts)

  if (!res.ok) {
    const text = await res.text().catch(() => '')
    let msg = `HTTP ${res.status}`
    try {
      const json = JSON.parse(text)
      if (json.errors) msg = Object.values(json.errors).flat().join(' | ')
      else if (json.title) msg = json.title
      else if (json.message) msg = json.message
      else msg = text || msg
    } catch {
      if (text) msg = text
    }
    throw new Error(msg)
  }

  if (res.status === 204) return null
  return res.json().catch(() => null)
}

export const vehiclesApi = {
  getAll:               ()         => req('/vehicles'),
  getById:              (id)       => req(`/vehicles/${id}`),
  getFuelStats:         (id)       => req(`/vehicles/${id}/FuelStatistics`),
  getMaintenanceStatus: (id)       => req(`/vehicles/${id}/maintenance-status`),
  create:               (data)     => req('/vehicles',                          { method: 'POST',  body: data }),
  update:               (id, data) => req(`/vehicles/${id}`,                    { method: 'PUT',   body: data }),
  delete:               (id)       => req(`/vehicles/${id}`,                    { method: 'DELETE' }),
  calibrateOdometer:    (id, data) => req(`/vehicles/${id}/calibrate-odometer`, { method: 'PATCH', body: data }),
  startTrip:            (id)       => req(`/vehicles/${id}/start-trip`,         { method: 'POST' }),
  endTrip:              (id)       => req(`/vehicles/${id}/end-trip`,           { method: 'POST' }),
}

export const fuelingApi = {
  getByVehicle: (vehicleId) => req(`/fuelingevents/Vehicle/${vehicleId}`),
  create:       (data)      => req('/fuelingevents',       { method: 'POST',   body: data }),
  delete:       (id)        => req(`/fuelingevents/${id}`, { method: 'DELETE' }),
}

export const maintenanceApi = {
  getByVehicle: (vehicleId) => req(`/maintenanceevents/Vehicle/${vehicleId}`),
  create:       (data)      => req('/maintenanceevents',       { method: 'POST',   body: data }),
  delete:       (id)        => req(`/maintenanceevents/${id}`, { method: 'DELETE' }),
}

export const maintenanceTypesApi = {
  getAll: () => req('/maintenancetypes'),
}
