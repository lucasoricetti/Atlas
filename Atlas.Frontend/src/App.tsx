import { Suspense, lazy, useEffect, type ReactNode } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import RequireAuth from "./auth/RequireAuth";
import Layout from "./components/Layout";
import AccessDenied from "./pages/AccessDenied";
const DashboardPage = lazy(() => import("./pages/DashboardPage"));
const AssetsPage = lazy(() => import("./pages/AssetsPage"));
const DivisionsPage = lazy(() => import("./pages/DivisionsPage"));
const ServicesPage = lazy(() => import("./pages/ServicesPage"));
const SettingsPage = lazy(() => import("./pages/SettingsPage"));
const ContractsPage = lazy(() => import("./pages/ContractsPage"));
const SuppliersPage = lazy(() => import("./pages/SuppliersPage"));
const LoginTypesPage = lazy(() => import("./pages/LoginTypesPage"));
const CloudProvidersPage = lazy(() => import("./pages/CloudProvidersPage"));
const VirtualMachinesPage = lazy(() => import("./pages/VirtualMachinesPage"));
const GraphExplorerPage = lazy(() => import("./pages/GraphExplorerPage"));
const DivisionUsesAssetsPage = lazy(() => import("./pages/DivisionUsesAssetsPage"));
const DivisionOwnsAssetsPage = lazy(() => import("./pages/DivisionOwnsAssetsPage"));
const AssetDependsOnAssetPage = lazy(() => import("./pages/AssetDependsOnAssetPage"));
const AssetDependsOnServicePage = lazy(() => import("./pages/AssetDependsOnServicePage"));
const AssetUsedByDivisionPage = lazy(() => import("./pages/AssetUsedByDivisionPage"));
const AssetOwnedByDivisionPage = lazy(() => import("./pages/AssetOwnedByDivisionPage"));
const AssetHasSettingsPage = lazy(() => import("./pages/AssetHasSettingsPage"));
const AssetHasLoginTypePage = lazy(() => import("./pages/AssetHasLoginTypePage"));
const ServiceDependsOnServicePage = lazy(() => import("./pages/ServiceDependsOnServicePage"));
const ServiceRequiredByAssetPage = lazy(() => import("./pages/ServiceRequiredByAssetPage"));
const ServiceHasContractsPage = lazy(() => import("./pages/ServiceHasContractsPage"));
const ServiceHostedByCloudProviderPage = lazy(() => import("./pages/ServiceHostedByCloudProviderPage"));
const ServiceHostedByVirtualMachinePage = lazy(() => import("./pages/ServiceHostedByVirtualMachinePage"));
const ServiceHasSettingsPage = lazy(() => import("./pages/ServiceHasSettingsPage"));
const SettingUsedByAssetPage = lazy(() => import("./pages/SettingUsedByAssetPage"));
const SettingUsedByServicePage = lazy(() => import("./pages/SettingUsedByServicePage"));
const LoginTypeUsedByAssetPage = lazy(() => import("./pages/LoginTypeUsedByAssetPage"));
const ContractProvidedBySupplierPage = lazy(() => import("./pages/ContractProvidedBySupplierPage"));
const ContractUsedByServicePage = lazy(() => import("./pages/ContractUsedByServicePage"));
const SupplierProvidesContractPage = lazy(() => import("./pages/SupplierProvidesContractPage"));
const CloudProviderHostsServicesPage = lazy(() => import("./pages/CloudProviderHostsServicesPage"));
const VirtualMachineHostsServicesPage = lazy(() => import("./pages/VirtualMachineHostsServicesPage"));
const LoginPage = lazy(() => import("./pages/LoginPage"));
const ProcessesPage = lazy(() => import("./pages/ProcessesPage"));
const DivisionUsesProcessPage = lazy(() => import("./pages/DivisionUsesProcessPage"));
const DivisionOwnsProcessPage = lazy(() => import("./pages/DivisionOwnsProcessPage"));
const ProcessInvolvesAssetPage = lazy(() => import("./pages/ProcessInvolvesAssetPage"));
const ProcessInvolvesServicePage = lazy(() => import("./pages/ProcessInvolvesServicePage"));
const ProcessHasSettingPage = lazy(() => import("./pages/ProcessHasSettingPage"));
const ProcessUsedByDivisionPage = lazy(() => import("./pages/ProcessUsedByDivisionPage"));
const ProcessOwnedByDivisionPage = lazy(() => import("./pages/ProcessOwnedByDivisionPage"));
const AssetInvolvedByProcessPage = lazy(() => import("./pages/AssetInvolvedByProcessPage"));
const ServiceInvolvedByProcessPage = lazy(() => import("./pages/ServiceInvolvedByProcessPage"));
const SettingUsedByProcessPage = lazy(() => import("./pages/SettingUsedByProcessPage"));
const AcnMacroAreasPage = lazy(() => import("./pages/AcnMacroAreasPage"));
const AcnMacroAreaClassifiesProcessesPage = lazy(() => import("./pages/AcnMacroAreaClassifiesProcessesPage"));
const ProcessClassifiedAsAcnMacroAreasPage = lazy(() => import("./pages/ProcessClassifiedAsAcnMacroAreasPage"));


const renderLazyRoute = (element: ReactNode) => (
  <Suspense fallback={<div className="page"><div className="card">Loading page...</div></div>}>
    {element}
  </Suspense>
);


export default function App() {
  useEffect(() => {
    document.title = "Atlas";
  }, []);

  return (
    <Routes>
      <Route path="/login" element={renderLazyRoute(<LoginPage />)} />

      <Route element={<RequireAuth />}>
        <Route path="/access-denied" element={renderLazyRoute(<AccessDenied />)} />

        <Route element={<Layout />}>
          <Route path="/" element={renderLazyRoute(<DashboardPage />)} />
          <Route path="/assets" element={renderLazyRoute(<AssetsPage />)} />
          <Route path="/divisions" element={renderLazyRoute(<DivisionsPage />)} />
          <Route path="/services" element={renderLazyRoute(<ServicesPage />)} />
          <Route path="/settings" element={renderLazyRoute(<SettingsPage />)} />
          <Route path="/contracts" element={renderLazyRoute(<ContractsPage />)} />
          <Route path="/suppliers" element={renderLazyRoute(<SuppliersPage />)} />
          <Route path="/login-types" element={renderLazyRoute(<LoginTypesPage />)} />
          <Route path="/cloud-providers" element={renderLazyRoute(<CloudProvidersPage />)} />
          <Route path="/virtual-machines" element={renderLazyRoute(<VirtualMachinesPage />)} />
          <Route path="/processes" element={renderLazyRoute(<ProcessesPage />)} />
          <Route path="/acn-macro-areas" element={renderLazyRoute(<AcnMacroAreasPage />)} />
          <Route path="/graph-explorer" element={renderLazyRoute(<GraphExplorerPage />)} />

          <Route path="/divisions/:id/uses-assets" element={renderLazyRoute(<DivisionUsesAssetsPage />)} />
          <Route path="/divisions/:id/owns-assets" element={renderLazyRoute(<DivisionOwnsAssetsPage />)} />
          <Route path="/assets/:id/asset-dependencies" element={renderLazyRoute(<AssetDependsOnAssetPage />)} />
          <Route path="/assets/:id/service-dependencies" element={renderLazyRoute(<AssetDependsOnServicePage />)} />
          <Route path="/assets/:id/used-by-divisions" element={renderLazyRoute(<AssetUsedByDivisionPage />)} />
          <Route path="/assets/:id/owned-by-divisions" element={renderLazyRoute(<AssetOwnedByDivisionPage />)} />
          <Route path="/assets/:id/has-settings" element={renderLazyRoute(<AssetHasSettingsPage />)} />
          <Route path="/assets/:id/has-login-types" element={renderLazyRoute(<AssetHasLoginTypePage />)} />
          <Route path="/services/:id/service-dependencies" element={renderLazyRoute(<ServiceDependsOnServicePage />)} />
          <Route path="/services/:id/required-by-assets" element={renderLazyRoute(<ServiceRequiredByAssetPage />)} />
          <Route path="/services/:id/has-contracts" element={renderLazyRoute(<ServiceHasContractsPage />)} />
          <Route path="/services/:id/hosted-by-cloud-providers" element={renderLazyRoute(<ServiceHostedByCloudProviderPage />)} />
          <Route path="/services/:id/hosted-by-virtual-machines" element={renderLazyRoute(<ServiceHostedByVirtualMachinePage />)} />
          <Route path="/services/:id/has-settings" element={renderLazyRoute(<ServiceHasSettingsPage />)} />
          <Route path="/settings/:id/used-by-assets" element={renderLazyRoute(<SettingUsedByAssetPage />)} />
          <Route path="/settings/:id/used-by-services" element={renderLazyRoute(<SettingUsedByServicePage />)} />
          <Route path="/login-types/:id/used-by-assets" element={renderLazyRoute(<LoginTypeUsedByAssetPage />)} />
          <Route path="/contracts/:id/provided-by-supplier" element={renderLazyRoute(<ContractProvidedBySupplierPage />)} />
          <Route path="/contracts/:id/used-by-services" element={renderLazyRoute(<ContractUsedByServicePage />)} />
          <Route path="/suppliers/:id/provides-contracts" element={renderLazyRoute(<SupplierProvidesContractPage />)} />
          <Route path="/cloud-providers/:id/hosts-services" element={renderLazyRoute(<CloudProviderHostsServicesPage />)} />
          <Route path="/virtual-machines/:id/hosts-services" element={renderLazyRoute(<VirtualMachineHostsServicesPage />)} />
          <Route path="/divisions/:id/uses-processes" element={renderLazyRoute(<DivisionUsesProcessPage />)} />
          <Route path="/divisions/:id/owns-processes" element={renderLazyRoute(<DivisionOwnsProcessPage />)} />
          <Route path="/processes/:id/involves-assets" element={renderLazyRoute(<ProcessInvolvesAssetPage />)} />
          <Route path="/processes/:id/involves-services" element={renderLazyRoute(<ProcessInvolvesServicePage />)} />
          <Route path="/processes/:id/has-settings" element={renderLazyRoute(<ProcessHasSettingPage />)} />
          <Route path="/processes/:id/used-by-divisions" element={renderLazyRoute(<ProcessUsedByDivisionPage />)} />
          <Route path="/processes/:id/owned-by-divisions" element={renderLazyRoute(<ProcessOwnedByDivisionPage />)} />
          <Route path="/assets/:id/involved-by-processes" element={renderLazyRoute(<AssetInvolvedByProcessPage />)} />
          <Route path="/services/:id/involved-by-processes" element={renderLazyRoute(<ServiceInvolvedByProcessPage />)} />
          <Route path="/settings/:id/used-by-processes" element={renderLazyRoute(<SettingUsedByProcessPage />)} />
          <Route path="/acn-macro-areas/:id/classifies" element={renderLazyRoute(<AcnMacroAreaClassifiesProcessesPage />)} />
          <Route path="/processes/:id/classified-as" element={renderLazyRoute(<ProcessClassifiedAsAcnMacroAreasPage />)} />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Route>
    </Routes>
  );
}