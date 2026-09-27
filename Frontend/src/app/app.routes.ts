import { Routes } from '@angular/router';
import { UserComponent } from './user/user.component';
import { RegistrationComponent } from './user/registration/registration.component';
import { LoginComponent } from './user/login/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { authGuard } from './shared/auth.guard';
import { HomeComponent } from './pages/home/home.component';
import { ContactComponent } from './pages/contact/contact.component';
import { PageComponent } from './pages/page.component';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { EditUserComponent } from './authorize/edit-user/edit-user.component';
import { ProfileMangementComponent } from './authorize/profile-mangement/profile-mangement.component';
import { ForbiddenComponent } from './forbidden/forbidden.component';
import { claimReq } from './shared/utils/claimReq-utils';
import { ServiceComponent } from './pages/service/service.component';
import { RegisterUserComponent } from './authorize/register-user/register-user.component';
import { EditRequestComponent } from './authorize/edit-request/edit-request.component';
import { CreateOrderComponent } from './authorize/create-order/create-order.component';
import { OverviewOrderComponent } from './authorize/overview-order/overview-order.component';
import { OverviewRequestComponent } from './authorize/overview-request/overview-request.component';
import { OverviewUserComponent } from './authorize/overview-user/overview-user.component';
import { EditOrderComponent } from './authorize/edit-order/edit-order.component';
import { OverviewInvoiceComponent } from './authorize/overview-invoice/overview-invoice.component';
import { EditInvoiceComponent } from './authorize/edit-invoice/edit-invoice.component';
import { MyOrderComponent } from './authorize/my-order/my-order.component';
import { MyInvoiceComponent } from './authorize/my-invoice/my-invoice.component';
import { VideoCallComponent } from './authorize/video-call/video-call.component';
import { ResetPasswordComponent } from './authorize/reset-password/reset-password.component';
import { PasswordChangeComponent } from './authorize/password-change/password-change.component';


export const routes: Routes = [
    {
        path: '', redirectTo: '/page/home', pathMatch: 'full'
    },
    {
        path: 'page', component: PageComponent, children: [
            { path: 'home', component: HomeComponent },
            {
                path: 'service', component: ServiceComponent
            },
            { path: 'contact', component: ContactComponent }
        ]
    },
    {
        path: 'user', component: UserComponent, children: [
            { path: 'signup', component: RegistrationComponent },
            { path: 'signin', component: LoginComponent },
        ]
    },
    {
        path: '', component: MainLayoutComponent, canActivate: [authGuard],
        canActivateChild: [authGuard],
        children: [
            {
                path: 'dashboard', component: DashboardComponent
            },
            {
                path: 'register-user', component: RegisterUserComponent
                , data: { claimReq: claimReq.adminOrEmployee }
            },
            {
                path: 'my-order', component: MyOrderComponent
                , data: { claimReq: claimReq.all }
            },
            {
                path: 'my-invoice/:id', component: MyInvoiceComponent
                , data: { claimReq: claimReq.all }
            },
            {
                path: 'create-order', component: CreateOrderComponent
                , data: { claimReq: claimReq.adminOrEmployee }
            },
            {
                path: 'overview-order', component: OverviewOrderComponent
                , data: { claimReq: claimReq.adminOrEmployee }
            },
            {
                path: 'overview-invoice', component: OverviewInvoiceComponent
                , data: { claimReq: claimReq.adminOrEmployee }
            },
            {
                path: 'edit-invoice/:id', component: EditInvoiceComponent
                , data: { claimReq: claimReq.adminOnly }
            },
            {
                path: 'edit-order/:id', component: EditOrderComponent
                , data: { claimReq: claimReq.adminOrEmployee }
            },
            {
                path: 'edit-user/:id', component: EditUserComponent
                , data: { claimReq: claimReq.adminOrEmployee }
            },
            {
                path: 'edit-request/:id', component: EditRequestComponent
                , data: { claimReq: claimReq.adminOrEmployee }
            },
            {
                path: 'overview-request', component: OverviewRequestComponent
                , data: { claimReq: claimReq.adminOrEmployee }
            },
            {
                path: 'overview-user', component: OverviewUserComponent
                , data: { claimReq: claimReq.adminOrEmployee }
            },
            {
                path: 'profile-mangement', component: ProfileMangementComponent
                , data: { claimReq: claimReq.all }
            },
            {
                path:'video-call',component:VideoCallComponent
                ,data:{claimReq:claimReq.adminOrEmployee}
            }
            ,{
                path:'reset-password',component:ResetPasswordComponent
                ,data:{claimReq:claimReq.adminOnly}
            },
            {
                path:'password-change',component:PasswordChangeComponent
                ,data:{claimReq:claimReq.all}
            }
            ,
            { path: 'forbidden', component: ForbiddenComponent }
        ]
    }

];
